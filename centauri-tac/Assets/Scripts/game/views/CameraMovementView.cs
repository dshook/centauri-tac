using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using strange.extensions.mediation.impl;
using System;
using System.Collections;
using System.Linq;

namespace ctac
{
    public class CameraMovementView : View
    {
        public bool dragEnabled = true;
        public bool zoomEnabled = true;
        public Vector4 camBounds = Vector4.zero;

        RaycastModel raycastModel;
        Camera cam;

        Vector3 dragOrigin;
        Vector3 mouseDiff;
        bool dragging = false;

        float zoomLevel = 1f;
        const float camPanSpeed = 2.5f;
        const float camPanThreshold = 0.2f;

        Vector3 upDownMoveDirection = new Vector3(1, 0, 1);
        Vector3 rightLeftMoveDirection = new Vector3(0.5f, 0, -0.5f);

        Vector3 rotateOrigin;
        bool rotateDragging = false;
        float rotateTimer = 0f;

        public void Init(RaycastModel rm)
        {
            cam = Camera.main;
            raycastModel = rm;
            cam.orthographicSize = CameraOrthoSize();
        }

        void Update()
        {
            rotateTimer += Time.deltaTime;

            cam.orthographicSize = Mathf.Lerp(CameraOrthoSize(), cam.orthographicSize, 0.5f);

            UpdateRotation();

            UpdateZoom();

            UpdateDragging();
        }

        void UpdateRotation()
        {
            if (CrossPlatformInputManager.GetAxis("Rotation") > 0.2)
            {
               KeyRotateCamera(true);
            }
            if (CrossPlatformInputManager.GetAxis("Rotation") < -0.2)
            {
               KeyRotateCamera(false);
            }

            var updateRotateOrigin = true;
            if (rotateDragging)
            {
                var mouseDiff = CrossPlatformInputManager.mousePosition - rotateOrigin;

                //Don't update our rotate origin when we've snapped to a position so rotation doesn't get stuck at the snap point
                updateRotateOrigin = RotateCamera(mouseDiff.x);
            }

            if (CrossPlatformInputManager.GetButton("Fire3"))
            {
                rotateDragging = true;
                if (updateRotateOrigin)
                {
                    rotateOrigin = CrossPlatformInputManager.mousePosition;
                }
            }
            if (CrossPlatformInputManager.GetButtonUp("Fire3"))
            {
                rotateDragging = false;
            }

        }

        Vector3 rotateWorldPosition = Vector3.zero;
        static readonly float[] snapPositions = new float[]{45f, 135f, 225f, 315f};
        float snapThreshold = 8f;

        //Returns whether or not the camera snapped
        bool RotateCamera(float amount) 
        {
            //find the point the camera is looking at on an imaginary plane at 0f height
            LinePlaneIntersection(out rotateWorldPosition, cam.transform.position, cam.transform.forward, Vector3.up, Vector3.zero);

            var destCameraAngle = (0.3f * amount);

            //then rotate around it
            cam.transform.RotateAround(rotateWorldPosition, Vector3.up, destCameraAngle);

            //snapping
            var camYRot = cam.transform.rotation.eulerAngles.y;
            float? amtToSnap = null;
            for (var i = 0; i < snapPositions.Length; i++)
            {
                if (Math.Abs(camYRot - snapPositions[i]) < snapThreshold)
                {
                    amtToSnap = snapPositions[i] - camYRot;
                }
            }
            if (amtToSnap.HasValue)
            {
                cam.transform.RotateAround(rotateWorldPosition, Vector3.up, amtToSnap.Value);
            }

            return !amtToSnap.HasValue;
        }

        private void KeyRotateCamera(bool rotateLeft)
        {
            if(rotateTimer < 1f) return;

            rotateTimer = 0f;

            //find the point the camera is looking at on an imaginary plane at 0f height
            LinePlaneIntersection(out rotateWorldPosition, cam.transform.position, cam.transform.forward, Vector3.up, Vector3.zero);

            //then rotate around it
            var destCameraAngle = rotateLeft ? -90f : 90f;

            //find the nearest snap angle and compensate our dest angle
            var finalCameraAngle = cam.transform.rotation.eulerAngles.y + destCameraAngle;
            if (finalCameraAngle < 0) { finalCameraAngle += 360; }
            if (finalCameraAngle > 360) { finalCameraAngle -= 360; }

            var closestSnap = snapPositions.OrderBy(p => Math.Abs(finalCameraAngle - p)).First();
            destCameraAngle += closestSnap - finalCameraAngle;

            StartCoroutine(SmoothRotateCamera(rotateWorldPosition, Vector3.up, destCameraAngle, 0.8f));
        }

        public IEnumerator SmoothRotateCamera(Vector3 point, Vector3 axis, float angle, float time)
        {
            var step = 0.0f; //non-smoothed
            var rate = 1.0f / time; //amount to increase non-smooth step by
            var smoothStep = 0.0f; //smooth step this time
            var lastStep = 0.0f; //smooth step last time
            while (step < 1.0)
            { // until we're done
                step += Time.deltaTime * rate; //increase the step
                smoothStep = Mathf.SmoothStep(0.0f, 1.0f, step); //get the smooth step
                cam.transform.RotateAround(point, axis, angle * (smoothStep - lastStep));
                lastStep = smoothStep; //store the smooth step
                yield return null;
            }
            //finish any left-over
            if (step > 1.0)
                cam.transform.RotateAround(point, axis, angle * (1.0f - lastStep));
        }


        void UpdateZoom()
        {
            if (!zoomEnabled) { return; }

            if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") > 0)
            {
                ZoomInOut(true);
            }
            if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") < 0)
            {
                ZoomInOut(false);
            }
        }

        float camZoomMax = 1.7f;
        float camZoomMin = 0.6f;
        float zoomSpeed = 0.10f;
        void ZoomInOut(bool zoomIn)
        {
            if (zoomIn)
            {
                zoomLevel = Math.Max(camZoomMin, zoomLevel - zoomSpeed);
            }
            else
            {
                zoomLevel = Math.Min(camZoomMax, zoomLevel + zoomSpeed);
            }
        }

        void UpdateDragging()
        {
            if (!dragEnabled)
            {
                dragging = false;
                return;
            }
            //When moving the view up or down we actually need to move the camera position in both x and z so it stays at the same height
            //include the fudge factor to get the mouse dragging right. There's probably a rotation to solve this properly
            upDownMoveDirection = cam.transform.forward.SetY(0).normalized * (1.2f + Math.Abs(cam.transform.forward.y));
            rightLeftMoveDirection = Quaternion.Euler(0, 90f, 0) * upDownMoveDirection;

            //Arrows
            //up
            if (CrossPlatformInputManager.GetAxis("Vertical")   >  camPanThreshold) { cam.transform.position += upDownMoveDirection * camPanSpeed * Time.deltaTime; }
            //right
            if (CrossPlatformInputManager.GetAxis("Horizontal") >  camPanThreshold) { cam.transform.position += rightLeftMoveDirection * camPanSpeed * Time.deltaTime; }
            //down
            if (CrossPlatformInputManager.GetAxis("Vertical")   < -camPanThreshold) { cam.transform.position -= upDownMoveDirection * camPanSpeed * Time.deltaTime; }
            //left
            if (CrossPlatformInputManager.GetAxis("Horizontal") < -camPanThreshold) { cam.transform.position -= rightLeftMoveDirection * camPanSpeed * Time.deltaTime; }


            if (CrossPlatformInputManager.GetButtonUp("Fire1"))
            {
                dragging = false;
                return;
            }

            if (dragging)
            {
                var mousePos = cam.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
                mouseDiff = mousePos - dragOrigin;
                //Convert a y movement in the cameras position to x & z movements
                //This prevents the camera from getting too high and going outside of the bounds
                cam.transform.position -= (mouseDiff.y * upDownMoveDirection) + mouseDiff.SetY(0);
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                //Check to make sure we didn't click on a card
                if (raycastModel.cardCanvasHit == null)
                {
                    dragOrigin = cam.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
                    dragging = true;
                }
            }

            if (camBounds != Vector4.zero)
            {
                var camPos = cam.transform.position;
                camPos.x = Mathf.Max(camPos.x, camBounds.x);
                camPos.x = Mathf.Min(camPos.x, camBounds.z);
                camPos.z = Mathf.Max(camPos.z, camBounds.y);
                camPos.z = Mathf.Min(camPos.z, camBounds.w);

                cam.transform.position = camPos;
            }
        }

        //move the camera so it's focused on a world point
        public void MoveToTile(Vector2 tilePos, float transitionTime = 0.5f)
        {
            Vector3 currentWorldPos;
            var destPosition = new Vector3(tilePos.x, 0, tilePos.y); //convert tile coords to full vec3
            LinePlaneIntersection(out currentWorldPos, cam.transform.position, cam.transform.forward, Vector3.up, Vector3.zero);

            //TODO: probably needs to be fixed for the y height?
            var finalPosition = cam.transform.position + destPosition - currentWorldPos;

            iTweenExtensions.MoveTo(cam.gameObject, finalPosition, transitionTime, 0f);
        }

        //Get the intersection between a line and a plane. 
        //If the line and plane are not parallel, the function outputs true, otherwise false.
        public bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
        {

            float length;
            float dotNumerator;
            float dotDenominator;
            Vector3 vector;
            intersection = Vector3.zero;

            //calculate the distance between the linePoint and the line-plane intersection point
            dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
            dotDenominator = Vector3.Dot(lineVec, planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;

                //create a vector from the linePoint to the intersection point
                vector = SetVectorLength(lineVec, length);

                //get the coordinates of the line-plane intersection point
                intersection = linePoint + vector;

                return true;
            }

            //output not valid
            else
            {
                return false;
            }
        }

        //create a vector of direction "vector" with length "size"
        public Vector3 SetVectorLength(Vector3 vector, float size)
        {
            //normalize the vector
            Vector3 vectorNormalized = Vector3.Normalize(vector);

            //scale the vector
            return vectorNormalized *= size;
        }

        float CameraOrthoSize()
        {
            //Adjust camera zoom based on screen size and zoom level
            return (Screen.height / 96.0f / 2.0f) * zoomLevel;
        }
    }
}

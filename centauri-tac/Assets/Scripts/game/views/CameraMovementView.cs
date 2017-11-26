using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using strange.extensions.mediation.impl;
using System;
using System.Collections;

namespace ctac
{
    public class CameraMovementView : View
    {

        RaycastModel raycastModel;
        Camera cam;

        public bool dragEnabled = true;
        public bool zoomEnabled = true;

        Vector3 dragOrigin;
        Vector3 mouseDiff;
        bool dragging = false;

        float zoomLevel = 1f;
        const float camPanSpeed = 0.3f;

        Vector3 upDownMoveDirection = new Vector3(1, 0, 1);
        Vector3 rightLeftMoveDirection = new Vector3(0.5f, 0, -0.5f);

        Vector3 rotateOrigin;
        bool rotateDragging = false;

        public void Init(RaycastModel rm)
        {
            cam = Camera.main;
            raycastModel = rm;
            cam.orthographicSize = CameraOrthoSize();
        }

        void Update()
        {
            cam.orthographicSize = Mathf.Lerp(CameraOrthoSize(), cam.orthographicSize, 0.5f);

            UpdateRotation();

            UpdateZoom();

            UpdateDragging();
        }

        void UpdateZoom()
        {
            if (zoomEnabled)
            {
                if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") > 0)
                {
                    ZoomInOut(true);
                }
                if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") < 0)
                {
                    ZoomInOut(false);
                }
            }
        }

        void UpdateRotation()
        {
            if (rotateDragging)
            {
                var mouseDiff = CrossPlatformInputManager.mousePosition - rotateOrigin;
                RotateCamera(mouseDiff.x);
            }

            //if (CrossPlatformInputManager.GetButtonDown("Fire3"))
            if (CrossPlatformInputManager.GetButton("Fire3"))
            {
                rotateDragging = true;
                rotateOrigin = CrossPlatformInputManager.mousePosition;
            }
            if (CrossPlatformInputManager.GetButtonUp("Fire3"))
            {
                rotateDragging = false;
            }

        }

        Vector3 rotateWorldPosition = Vector3.zero;
        private void RotateCamera(float amount) 
        {
            //find the point the camera is looking at on an imaginary plane at 0f height
            LinePlaneIntersection(out rotateWorldPosition, cam.transform.position, cam.transform.forward, Vector3.up, Vector3.zero);

            var destCameraAngle = (0.5f * amount);

            //then rotate around it
            cam.transform.RotateAround(rotateWorldPosition, Vector3.up, destCameraAngle);
        }

        public IEnumerator RotateCamera(Vector3 point, Vector3 axis, float angle, float time)
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

        private float camZoomMax = 1.7f;
        private float camZoomMin = 0.6f;
        private float zoomSpeed = 0.10f;
        private void ZoomInOut(bool zoomIn)
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

        private void UpdateDragging()
        {
            if (!dragEnabled)
            {
                dragging = false;
                return;
            }
            upDownMoveDirection = cam.transform.forward.SetY(0).normalized;
            //up
            if (CrossPlatformInputManager.GetAxis("Vertical") > 0.2)
            {
                cam.transform.position += upDownMoveDirection * camPanSpeed;
            }
            //down
            if (CrossPlatformInputManager.GetAxis("Vertical") < -0.2)
            {
                cam.transform.position -= upDownMoveDirection * camPanSpeed;
            }
            //right
            if (CrossPlatformInputManager.GetAxis("Horizontal") > 0.2)
            {
                cam.transform.position += rightLeftMoveDirection * camPanSpeed;
            }
            //left
            if (CrossPlatformInputManager.GetAxis("Horizontal") < -0.2)
            {
                cam.transform.position -= rightLeftMoveDirection * camPanSpeed;
            }

            if (CrossPlatformInputManager.GetButtonUp("Fire1"))
            {
                dragging = false;
                return;
            }

            if (dragging)
            {
                var mousePos = cam.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
                mouseDiff = mousePos - dragOrigin;
                cam.transform.position -= fixCameraMoveVector(mouseDiff);
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                if (raycastModel.cardCanvasHit == null)
                {
                    dragOrigin = cam.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
                    dragging = true;
                }
            }
        }

        //Convert a y movement in the cameras position to x & z movements
        //This prevents the camera from getting too high and going outside of the bounds
        Vector3 fixCameraMoveVector(Vector3 vec)
        {
            return (vec.y * upDownMoveDirection) + vec.SetY(0);
        }

        //move the camera so it's focused on a world point
        internal void MoveToTile(Vector2 tilePos, float transitionTime = 0.5f)
        {
            Vector3 currentWorldPos;
            var destPosition = new Vector3(tilePos.x, 0, tilePos.y); //convert tile coords to full vec3
            LinePlaneIntersection(out currentWorldPos, cam.transform.position, cam.transform.forward, Vector3.up, Vector3.zero);

            //TODO: probably needs to be fixed for the y height?
            var finalPosition = cam.transform.position + destPosition - currentWorldPos;

            //Camera.main.transform.position += finalPosition;

            iTweenExtensions.MoveTo(cam.gameObject, finalPosition, transitionTime, 0f);
        }

        //Get the intersection between a line and a plane. 
        //If the line and plane are not parallel, the function outputs true, otherwise false.
        public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
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
        public static Vector3 SetVectorLength(Vector3 vector, float size)
        {
            //normalize the vector
            Vector3 vectorNormalized = Vector3.Normalize(vector);

            //scale the vector
            return vectorNormalized *= size;
        }

        private float CameraOrthoSize()
        {
            //Adjust camera zoom based on screen size and zoom level
            return (Screen.height / 96.0f / 2.0f) * zoomLevel;
        }
    }
}

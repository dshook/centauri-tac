using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using strange.extensions.mediation.impl;
using System;
using strange.extensions.signal.impl;
using System.Collections;

namespace ctac
{
    public class CameraMovementView : View
    {
        private Vector3 dragOrigin;
        private Vector3 camOrigin;

        private Vector3 mouseDiff;
        private Vector3 amountToMove;
        private Vector3 move;

        private RaycastModel raycastModel;

        private bool dragEnabled = false;
        public bool zoomEnabled = true;

        bool dragging = false;
        float xSpeed = 10f;
        float ySpeed = 5f;

        float rotateTimer = 0f;

        float zoomLevel = 1f;

        public void Init(RaycastModel rm)
        {
            raycastModel = rm;
            Camera.main.orthographicSize = CameraOrthoSize();
        }

        void Update()
        {
            Camera.main.orthographicSize = Mathf.Lerp(CameraOrthoSize(), Camera.main.orthographicSize, 0.5f);

            rotateTimer += Time.deltaTime;

            if (CrossPlatformInputManager.GetAxis("Horizontal") > 0.2)
            {
                RotateCamera(true);
            }
            if (CrossPlatformInputManager.GetAxis("Horizontal") < -0.2)
            {
                RotateCamera(false);
            }

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

            UpdateDragging();
        }

        private float CameraOrthoSize()
        {
            //Adjust camera zoom based on screen size and zoom level
            return (Screen.height / 96.0f / 2.0f) * zoomLevel;
        }

        Vector3 rotateWorldPosition = Vector3.zero;
        private void RotateCamera(bool rotateLeft)
        {
            if(rotateTimer < 1f) return;

            rotateTimer = 0f;

            //find the point the camera is looking at on an imaginary plane at 0f height
            LinePlaneIntersection(out rotateWorldPosition, Camera.main.transform.position, Camera.main.transform.forward, Vector3.up, Vector3.zero);

            //then rotate around it
            var destCameraAngle = rotateLeft ? Camera.main.transform.rotation.y + -90 : Camera.main.transform.rotation.y + 90;

            StartCoroutine(RotateCamera(rotateWorldPosition, Vector3.up, destCameraAngle, 0.8f));
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
                Camera.main.transform.RotateAround(point, axis, angle * (smoothStep - lastStep));
                lastStep = smoothStep; //store the smooth step
                yield return null;
            }
            //finish any left-over
            if (step > 1.0)
                Camera.main.transform.RotateAround(point, axis, angle * (1.0f - lastStep));
        }

        private float camMax = 1.7f;
        private float camMin = 0.6f;
        private float zoomSpeed = 0.10f;
        private void ZoomInOut(bool zoomIn)
        {
            if (zoomIn)
            {
                zoomLevel = Math.Max(camMin, zoomLevel - zoomSpeed);
            }
            else
            {
                zoomLevel = Math.Min(camMax, zoomLevel + zoomSpeed);
            }
        }

        private void UpdateDragging()
        {
            if (!dragEnabled)
            {
                dragging = false;
                return;
            }

            if (CrossPlatformInputManager.GetButtonUp("Fire1"))
            {
                dragging = false;
                return;
            }

            if (dragging)
            {
                var mousePos = Camera.main.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
                mouseDiff = mousePos - dragOrigin;

                //TODO: speed is still not right
                amountToMove.Set(-mouseDiff.x * xSpeed * zoomLevel, -mouseDiff.y * ySpeed * zoomLevel, 0);
                amountToMove = Camera.main.transform.rotation * amountToMove;
                move = camOrigin + amountToMove;

                Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, move, 1f);
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                if (raycastModel.cardCanvasHit == null)
                {
                    dragOrigin = Camera.main.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
                    camOrigin = Camera.main.transform.position;
                    dragging = true;
                }
            }
        }

        internal void setDragEnabled(bool selected)
        {
            dragEnabled = selected;
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
    }
}

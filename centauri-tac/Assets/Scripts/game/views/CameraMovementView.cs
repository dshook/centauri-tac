using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using strange.extensions.mediation.impl;
using System;
using strange.extensions.signal.impl;

namespace ctac
{
    public class CameraMovementView : View
    {
        public Signal<float> CameraRotated = new Signal<float>();
        private Vector3 dragOrigin;
        private Vector3 camOrigin;

        private Vector3 mouseDiff;
        private Vector3 amountToMove;
        private Vector3 move;

        private bool cardSelected = false;

        bool dragging = false;
        float xSpeed = 10f;
        float ySpeed = 5f;

        float rotateTimer = 0f;

        void Update()
        {
            var s_baseOrthographicSize = Screen.height / 96.0f / 2.0f;
            Camera.main.orthographicSize = s_baseOrthographicSize;

            rotateTimer += Time.deltaTime;

            if (CrossPlatformInputManager.GetAxis("Horizontal") > 0.2)
            {
                RotateCamera(true);
            }
            if (CrossPlatformInputManager.GetAxis("Horizontal") < -0.2)
            {
                RotateCamera(false);
            }

            UpdateDragging();
        }

        private void RotateCamera(bool rotateLeft)
        {
            if(rotateTimer < 1f) return;

            rotateTimer = 0f;
            //find what we're looking at
            Ray camRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit objectHit;
            Vector3 rotateWorldPosition = Vector3.zero;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                rotateWorldPosition = objectHit.transform.position;
            }

            //then rotate around it
            Camera.main.transform.RotateAround(rotateWorldPosition, Vector3.up, rotateLeft ? -90 : 90);

            CameraRotated.Dispatch(Camera.main.transform.eulerAngles.y);
        }

        private void UpdateDragging()
        {
            if (cardSelected)
            {
                dragging = false;
                return;
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                //test click position to see if we hit the ground
                Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

                RaycastHit objectHit;
                if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
                {
                    if (objectHit.collider.gameObject.CompareTag("Tile"))
                    {
                        dragOrigin = Camera.main.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
                        camOrigin = Camera.main.transform.position;
                        dragging = true;
                        return;
                    }
                }
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
                amountToMove.Set(-mouseDiff.x * xSpeed, -mouseDiff.y * ySpeed, 0);
                amountToMove = Camera.main.transform.rotation * amountToMove;
                move = camOrigin + amountToMove;

                Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, move, 1f);
            }
        }

        internal void onCardSelected(bool selected)
        {
            cardSelected = selected;
        }
    }
}

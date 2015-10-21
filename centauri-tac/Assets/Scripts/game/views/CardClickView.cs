using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;
using System.Linq;

namespace ctac
{
    public class CardClickView : View
    {
        internal Signal<GameObject> clickSignal = new Signal<GameObject>();
        internal Signal<GameObject> activateSignal = new Signal<GameObject>();

        bool active = false;
        private Camera cardCamera;
        bool dragging = false;

        Ray camRay;
        internal void init()
        {
            active = true;
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == "CardCamera");
        }
        void Update()
        {
            if (active)
            {
                if (CrossPlatformInputManager.GetButtonDown("Fire1"))
                {
                    TestSelection();
                }

                if (dragging && CrossPlatformInputManager.GetButtonUp("Fire1"))
                {
                    TestActivate();
                }

                //right click et al deselects
                if (CrossPlatformInputManager.GetButtonDown("Fire2"))
                {
                    clickSignal.Dispatch(null);
                }

                //if (camRay.origin != null)
                //{
                //    Debug.DrawLine(camRay.origin, Quaternion.Euler(camRay.direction) * camRay.origin * Constants.cameraRaycastDist, Color.red, 10f);
                //}
            }
        }

        void TestSelection()
        {
            var viewportPoint = cardCamera.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
            camRay = cardCamera.ViewportPointToRay(viewportPoint);

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                clickSignal.Dispatch(objectHit.collider.gameObject);
                dragging = true;
            }
            else
            {
                clickSignal.Dispatch(null);
                dragging = false;
            }
        }

        void TestActivate()
        {
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                activateSignal.Dispatch(objectHit.collider.gameObject);
                clickSignal.Dispatch(null);
                dragging = false;
            }
            else
            {
                activateSignal.Dispatch(null);
            }
        }

    }
}


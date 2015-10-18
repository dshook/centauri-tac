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

        bool active = false;
        private Camera cardCamera;

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

                //right click et al deselects
                if (CrossPlatformInputManager.GetButtonDown("Fire2") || CrossPlatformInputManager.GetButtonUp("Fire1"))
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
            }
            else
            {
                clickSignal.Dispatch(null);
            }
        }

    }
}


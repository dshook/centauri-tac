using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;
using System.Linq;

namespace ctac
{
    public class CardHoverView : View
    {
        internal Signal<GameObject> hoverSignal = new Signal<GameObject>();

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
                TestSelection();
            }
        }

        void TestSelection()
        {
            var viewportPoint = cardCamera.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
            camRay = cardCamera.ViewportPointToRay(viewportPoint);

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                hoverSignal.Dispatch(objectHit.collider.gameObject);
            }
            else
            {
                hoverSignal.Dispatch(null);
            }
        }
    }
}


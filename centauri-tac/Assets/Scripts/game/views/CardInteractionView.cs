using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;
using System.Linq;

namespace ctac
{
    public class CardInteractionView : View
    {
        internal Signal<GameObject> clickSignal = new Signal<GameObject>();
        internal Signal<GameObject> activateSignal = new Signal<GameObject>();
        internal Signal<GameObject> hoverSignal = new Signal<GameObject>();

        bool active = false;
        private Camera cardCamera;
        bool dragging = false;
        bool targeting = false;

        Ray camRay;
        int cardCanvasLayer = -1;
        internal void init()
        {
            active = true;
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == Constants.cardCamera);
            cardCanvasLayer = LayerMask.GetMask(Constants.cardCanvas);
        }
        void Update()
        {
            if (!active) return;
            
            var hoverHit = TestSelection();

            if (hoverHit.HasValue)
            {
                hoverSignal.Dispatch(hoverHit.Value.collider.gameObject);
            }
            else
            {
                hoverSignal.Dispatch(null);
            }

            if ((targeting || dragging) && CrossPlatformInputManager.GetButtonUp("Fire1"))
            {
                TestActivate();
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                if (hoverHit.HasValue)
                {
                    clickSignal.Dispatch(hoverHit.Value.collider.gameObject);
                    dragging = true;
                }
                else
                {
                    clickSignal.Dispatch(null);
                    dragging = false;
                }
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

        RaycastHit? TestSelection()
        {
            var viewportPoint = cardCamera.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
            camRay = cardCamera.ViewportPointToRay(viewportPoint);

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist, cardCanvasLayer))
            {
                return objectHit;
            }
            else
            {
                return null;
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

        public void StartTarget()
        {
            targeting = true;
        }
        public void EndTarget()
        {
            targeting = false;
        }

    }
}


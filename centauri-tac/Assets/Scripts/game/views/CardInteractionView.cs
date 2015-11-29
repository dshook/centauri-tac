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
        internal Signal<GameObject, float> hoverSignal = new Signal<GameObject, float>();

        bool active = false;
        private Camera cardCamera;
        bool dragging = false;
        float hoverTime = 0f;

        Ray camRay;
        int cardCanvasLayer = -1;
        internal void init()
        {
            active = true;
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == "CardCamera");
            cardCanvasLayer = LayerMask.GetMask("cardCanvas");
        }
        void Update()
        {
            if (active)
            {
                var hoverHit = TestSelection();

                if (hoverHit.HasValue)
                {
                    hoverSignal.Dispatch(hoverHit.Value.collider.gameObject, hoverTime);
                    hoverTime += Time.deltaTime;
                }
                else
                {
                    hoverSignal.Dispatch(null, hoverTime);
                    hoverTime = 0f;
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

    }
}

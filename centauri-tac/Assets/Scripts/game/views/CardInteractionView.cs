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

        GameObject draggedObject;
        bool active = false;
        private Camera cardCamera;
        bool dragging = false;
        float dragTimer = 0f;
        float dragMin = 0.8f;

        Ray camRay;
        int cardCanvasLayer = -1;
        internal void init()
        {
            active = true;
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == Constants.cardCamera);
            cardCanvasLayer = LayerMask.GetMask(Constants.cardCanvas);
            clickSignal.AddListener(onClick);
        }

        void onClick(GameObject g)
        {
            draggedObject = g;
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

            if (CrossPlatformInputManager.GetButton("Fire1"))
            {
                dragTimer += Time.deltaTime;
                if (dragTimer > dragMin)
                {
                    dragging = true;
                }
            }

            if (dragging && CrossPlatformInputManager.GetButtonUp("Fire1"))
            {
                TestActivate();
                dragging = false;
                dragTimer = 0f;
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                if (draggedObject == null || !TestActivate())
                {
                    if (hoverHit.HasValue)
                    {
                        clickSignal.Dispatch(hoverHit.Value.collider.gameObject);
                    }
                    else
                    {
                        clickSignal.Dispatch(null);
                    }
                }
            }

            //right click et al deselects
            if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            {
                clickSignal.Dispatch(null);
                dragging = false;
                dragTimer = 0f;
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

        bool TestActivate()
        {
            bool hit = false;
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                hit = true;
                activateSignal.Dispatch(objectHit.collider.gameObject);
                clickSignal.Dispatch(null);
            }
            else
            {
                activateSignal.Dispatch(null);
            }
            return hit;
        }
    }
}


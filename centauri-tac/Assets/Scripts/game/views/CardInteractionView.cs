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
        RaycastModel raycastModel;

        GameObject draggedObject;
        bool active = false;
        bool dragging = false;
        float dragTimer = 0f;
        float dragMin = 0.8f;

        internal void init(RaycastModel rm)
        {
            active = true;
            clickSignal.AddListener(onClick);
            raycastModel = rm;
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
            return raycastModel.cardCanvasHit;
        }

        bool TestActivate()
        {
            bool hit = false;
            if (raycastModel.worldHit.HasValue)
            {
                hit = true;
                activateSignal.Dispatch(raycastModel.worldHit.Value.collider.gameObject);
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


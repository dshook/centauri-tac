using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class CardInteractionView : View
    {
        internal Signal<GameObject, Vector3> clickSignal = new Signal<GameObject, Vector3>();
        internal Signal<GameObject> activateSignal = new Signal<GameObject>();
        internal Signal<GameObject> hoverSignal = new Signal<GameObject>();
        RaycastModel raycastModel;

        GameObject draggedObject;
        bool active = false;
        float dragTimer = 0f;
        float dragMin = 0.10f;

        internal void init(RaycastModel rm)
        {
            active = true;
            clickSignal.AddListener(onClick);
            raycastModel = rm;
        }

        void onClick(GameObject g, Vector3 v)
        {
            draggedObject = g;
        }

        void Update()
        {
            if (!active) return;
            
            var hoverHit = raycastModel.cardCanvasHit;

            //check to see if a card in a hand has been hovered
            //also check to see if the hit object has been destroyed in the meantime
            if (hoverHit.HasValue 
                && hoverHit.Value.collider != null
                && hoverHit.Value.collider.gameObject.transform.parent.name == "cardCanvas"
            )
            {
                hoverSignal.Dispatch(hoverHit.Value.collider.gameObject);
            }
            else
            {
                hoverSignal.Dispatch(null);
            }

            if (draggedObject != null)
            {
                dragTimer += Time.deltaTime;
            }

            if (CrossPlatformInputManager.GetButtonUp("Fire1")) {
                if (draggedObject != null && dragTimer > dragMin)
                {
                    TestActivate();
                }
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                //if we're already dragging, test the activate, otherwise start dragging
                if (draggedObject != null && dragTimer > dragMin)
                {
                    TestActivate();
                }
                else
                {
                    if (hoverHit.HasValue)
                    {
                        clickSignal.Dispatch(hoverHit.Value.collider.gameObject, hoverHit.Value.point);
                    }
                    else
                    {
                        clickSignal.Dispatch(null, Vector3.zero);
                    }
                }
            }

            //right click et al deselects
            if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            {
                clickSignal.Dispatch(null, Vector3.zero);
                dragTimer = 0f;
            }

            //if (camRay.origin != null)
            //{
            //    Debug.DrawLine(camRay.origin, Quaternion.Euler(camRay.direction) * camRay.origin * Constants.cameraRaycastDist, Color.red, 10f);
            //}
        }

        bool TestActivate()
        {
            bool hit = false;
            if (raycastModel.worldHit.HasValue)
            {
                hit = true;
                activateSignal.Dispatch(raycastModel.worldHit.Value.collider.gameObject);
            }
            else
            {
                activateSignal.Dispatch(null);
                clickSignal.Dispatch(null, Vector3.zero);
            }
            //implicitly stop dragging after activating since we only test activation when we want to stop dragging
            draggedObject = null;
            dragTimer = 0f;
            return hit;
        }
    }
}


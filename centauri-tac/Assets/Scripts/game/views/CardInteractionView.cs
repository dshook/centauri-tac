using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class CardInteractionView : View
    {
        [Inject] public GameInputStatusModel gameInputStatus { get; set; }

        internal Signal<GameObject, Vector3> cardClickSignal = new Signal<GameObject, Vector3>();
        internal Signal<GameObject> activateSignal = new Signal<GameObject>();
        internal Signal<GameObject> hoverSignal = new Signal<GameObject>();
        RaycastModel raycastModel;

        GameObject draggedCard;
        bool active = false;
        float dragTimer = 0f;
        float dragMin = 0.10f;

        internal void init(RaycastModel rm)
        {
            active = true;
            cardClickSignal.AddListener(onCardClick);
            raycastModel = rm;
        }

        void onCardClick(GameObject g, Vector3 v)
        {
            draggedCard = g;
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

            if (draggedCard != null)
            {
                dragTimer += Time.deltaTime;
            }

            if (gameInputStatus.inputEnabled && CrossPlatformInputManager.GetButtonUp("Fire1")) {
                if (draggedCard != null && dragTimer > dragMin)
                {
                    TestActivate();
                }
            }

            if (gameInputStatus.inputEnabled && CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                //if we're already dragging, test the activate, otherwise start dragging
                if (draggedCard != null && dragTimer > dragMin)
                {
                    TestActivate();
                }
                else
                {
                    if (hoverHit.HasValue)
                    {
                        cardClickSignal.Dispatch(hoverHit.Value.collider.gameObject, hoverHit.Value.point);
                    }
                    else
                    {
                        cardClickSignal.Dispatch(null, Vector3.zero);
                    }
                }
            }

            //right click et al deselects
            if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            {
                cardClickSignal.Dispatch(null, Vector3.zero);
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
            if (raycastModel.tile != null)
            {
                hit = true;
                activateSignal.Dispatch(raycastModel.tile.gameObject);
            }
            else
            {
                activateSignal.Dispatch(null);
                cardClickSignal.Dispatch(null, Vector3.zero);
            }
            //if (raycastModel.worldHit.HasValue)
            //{
            //    hit = true;
            //    activateSignal.Dispatch(raycastModel.worldHit.Value.collider.gameObject);
            //}
            //else
            //{
            //    activateSignal.Dispatch(null);
            //    clickSignal.Dispatch(null, Vector3.zero);
            //}
            return hit;
        }

        internal void ClearDrag()
        {
            draggedCard = null;
            dragTimer = 0f;
        }
    }
}


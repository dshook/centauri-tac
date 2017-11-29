using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class CardsMenuInteractionView : View
    {
        internal Signal<GameObject, Vector3> clickSignal = new Signal<GameObject, Vector3>();
        internal Signal<GameObject> hoverSignal = new Signal<GameObject>();
        RaycastModel raycastModel;

        bool active = false;

        float dragAccumulator = 0f;
        bool isDragging = false;
        Vector2 lastDragPos = Vector2.zero;

        internal void init(RaycastModel rm)
        {
            active = true;
            raycastModel = rm;
        }

        void Update()
        {
            if (!active) return;
            
            var hoverHit = raycastModel.cardCanvasHit;

            if (hoverHit.HasValue && hoverHit.Value.collider != null )
            {
                hoverSignal.Dispatch(hoverHit.Value.collider.gameObject);
            }
            else
            {
                hoverSignal.Dispatch(null);
            }

            if (isDragging)
            {
                dragAccumulator += Vector2.Distance(lastDragPos, CrossPlatformInputManager.mousePosition);
                lastDragPos = CrossPlatformInputManager.mousePosition;
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                isDragging = true;
                lastDragPos = CrossPlatformInputManager.mousePosition;
                dragAccumulator = 0f;
            }

            if (CrossPlatformInputManager.GetButtonUp("Fire1")) {
                isDragging = false;
                if (hoverHit.HasValue && hoverHit.Value.collider != null && dragAccumulator < Constants.dragDistThreshold)
                {
                    clickSignal.Dispatch(hoverHit.Value.collider.gameObject, hoverHit.Value.point);
                }
                else
                {
                    clickSignal.Dispatch(null, Vector3.zero);
                }
            }

            //right click et al deselects
            if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            {
                clickSignal.Dispatch(null, Vector3.zero);
            }
        }
    }
}


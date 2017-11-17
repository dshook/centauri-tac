using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class ClickModel
    {
        public GameObject clickedObject { get; set; }
        public Tile tile { get; set; }
        public PieceView piece { get; set; }
        public bool isDrag { get; set; }
    }

    public class CardClickModel
    {
        public GameObject clickedCard { get; set; }
        public Vector3 position { get; set; }
        public bool isUp { get; set; }
    }

    public class ClickView : View
    {
        [Inject] public GameInputStatusModel gameInputStatus { get; set; }

        internal Signal<ClickModel> clickSignal = new Signal<ClickModel>();
        internal Signal<CardClickModel> cardClickSignal = new Signal<CardClickModel>();
        internal Signal<GameObject> hoverSignal = new Signal<GameObject>();
        RaycastModel raycastModel;

        bool active = false;
        const float singleClickThreshold = 0.5f;

        //how far has the mouse been dragged total
        float dragAccumulator = 0f;
        const float dragDistThreshold = 10f;
        bool isDragging = false;
        Vector2 lastDragPos = Vector2.zero;

        internal void init(RaycastModel rm)
        {
            active = true;
            raycastModel = rm;
        }

        void Update()
        {
            if (!active || !gameInputStatus.inputEnabled) { return; }

            //check to see if a card in a hand has been hovered
            //also check to see if the hit object has been destroyed in the meantime
            var canvasHoverHit = raycastModel.cardCanvasHit;
            if (canvasHoverHit.HasValue 
                && canvasHoverHit.Value.collider != null
                //make sure it's not a deck card hover
                && canvasHoverHit.Value.collider.gameObject.transform.parent.name == "cardCanvas" 
            )
            {
                hoverSignal.Dispatch(canvasHoverHit.Value.collider.gameObject);
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

                if (canvasHoverHit.HasValue)
                {
                    cardClickSignal.Dispatch(new CardClickModel(){
                        clickedCard = canvasHoverHit.Value.collider.gameObject,
                        position = canvasHoverHit.Value.point,
                        isUp = false
                    });
                }
                else
                {
                    //cardClickSignal.Dispatch(null);
                }
            }

            if (CrossPlatformInputManager.GetButtonUp("Fire1")) {
                isDragging = false;
                if (canvasHoverHit.HasValue)
                {
                    cardClickSignal.Dispatch(new CardClickModel(){
                        clickedCard = canvasHoverHit.Value.collider.gameObject,
                        position = canvasHoverHit.Value.point,
                        isUp = true
                    });
                }
                TestWorldHit(true);
            }

            //right click et al deselects
            if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            {
                isDragging = false;
                cardClickSignal.Dispatch(null);
                clickSignal.Dispatch(null);
            }

            //if (camRay.origin != null)
            //{
            //    Debug.DrawLine(camRay.origin, Quaternion.Euler(camRay.direction) * camRay.origin * Constants.cameraRaycastDist, Color.red, 10f);
            //}
        }

        void TestWorldHit(bool isUp)
        {
            if (raycastModel.worldHit.HasValue && !raycastModel.cardCanvasHit.HasValue)
            {
                clickSignal.Dispatch(new ClickModel() {
                    clickedObject = raycastModel.worldHit.Value.collider.gameObject,
                    piece = raycastModel.piece,
                    tile = raycastModel.tile,
                    isDrag = dragAccumulator > dragDistThreshold
                });
            }
            else if(!raycastModel.cardCanvasHit.HasValue)
            {
                clickSignal.Dispatch(null);
            }
        }
    }
}


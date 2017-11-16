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
        public bool isUp { get; set; }
        public float? clickTime { get; set; }
    }

    public class ClickView : View
    {
        [Inject] public GameInputStatusModel gameInputStatus { get; set; }

        internal Signal<ClickModel> clickSignal = new Signal<ClickModel>();

        internal Signal<GameObject, Vector3> cardClickSignal = new Signal<GameObject, Vector3>();
        internal Signal<GameObject> activateSignal = new Signal<GameObject>();
        internal Signal<GameObject> hoverSignal = new Signal<GameObject>();
        RaycastModel raycastModel;

        bool active = false;
        GameObject draggedCard;
        float dragTimer = 0f;
        float dragMin = 0.10f;

        internal void init(RaycastModel rm)
        {
            active = true;
            raycastModel = rm;
            cardClickSignal.AddListener(onCardClick);
        }

        float clickTimeAccum = 0f;

        void onCardClick(GameObject g, Vector3 v)
        {
            draggedCard = g;
        }

        void Update()
        {
            if (!active || !gameInputStatus.inputEnabled) { return; }

            clickTimeAccum += Time.deltaTime;

            if (CrossPlatformInputManager.GetButtonDown("Fire1") )
            {
                clickTimeAccum = 0f;
                TestSelection(false, null);
            }
            if (CrossPlatformInputManager.GetButtonUp("Fire1"))
            {
                TestSelection(true, clickTimeAccum);
            }

            //right click et al deselects
            if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            {
                clickSignal.Dispatch(new ClickModel() { isUp = false });
            }



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

        void TestSelection(bool isUp, float? time)
        {
            if (raycastModel.worldHit.HasValue && !raycastModel.cardCanvasHit.HasValue)
            {
                clickSignal.Dispatch(new ClickModel() {
                    clickedObject = raycastModel.worldHit.Value.collider.gameObject,
                    piece = raycastModel.piece,
                    tile = raycastModel.tile,
                    isUp = isUp,
                    clickTime = time
                });
            }
            else if(!raycastModel.cardCanvasHit.HasValue)
            {
                clickSignal.Dispatch(new ClickModel() { isUp = isUp });
            }
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


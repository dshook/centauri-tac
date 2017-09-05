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
        RaycastModel raycastModel;

        bool active = false;

        internal void init(RaycastModel rm)
        {
            active = true;
            raycastModel = rm;
        }

        float clickTimeAccum = 0f;

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

    }
}


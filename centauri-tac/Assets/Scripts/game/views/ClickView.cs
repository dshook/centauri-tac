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
    }

    public class ClickView : View
    {
        internal Signal<ClickModel> clickSignal = new Signal<ClickModel>();
        RaycastModel raycastModel;

        bool active = false;

        internal void init(RaycastModel rm)
        {
            active = true;
            raycastModel = rm;
        }

        void Update()
        {
            if (active)
            {
                if (CrossPlatformInputManager.GetButtonDown("Fire1") )
                {
                    TestSelection(false);
                }
                if (CrossPlatformInputManager.GetButtonUp("Fire1"))
                {
                    TestSelection(true);
                }

                //right click et al deselects
                if (CrossPlatformInputManager.GetButtonDown("Fire2"))
                {
                    clickSignal.Dispatch(new ClickModel() { isUp = false });
                }
            }
        }

        void TestSelection(bool isUp)
        {
            if (raycastModel.worldHit.HasValue)
            {
                clickSignal.Dispatch(new ClickModel() {
                    clickedObject = raycastModel.worldHit.Value.collider.gameObject,
                    piece = raycastModel.piece,
                    tile = raycastModel.tile,
                    isUp = isUp
                });
            }
            else
            {
                clickSignal.Dispatch(new ClickModel() { isUp = isUp });
            }
        }

    }
}


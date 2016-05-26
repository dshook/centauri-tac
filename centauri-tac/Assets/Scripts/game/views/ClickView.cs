using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class ClickView : View
    {
        internal Signal<GameObject, bool> clickSignal = new Signal<GameObject, bool>();
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
                    clickSignal.Dispatch(null, false);
                }
            }
        }

        void TestSelection(bool isUp)
        {
            if (raycastModel.worldHit.HasValue)
            {
                clickSignal.Dispatch(raycastModel.worldHit.Value.collider.gameObject, isUp);
            }
            else
            {
                clickSignal.Dispatch(null, isUp);
            }
        }

    }
}


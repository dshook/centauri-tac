using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class ClickView : View
    {
        internal Signal<GameObject> clickSignal = new Signal<GameObject>();
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
                if (CrossPlatformInputManager.GetButtonDown("Fire1") || CrossPlatformInputManager.GetButtonUp("Fire1"))
                {
                    TestSelection();
                }

                //right click et al deselects
                if (CrossPlatformInputManager.GetButtonDown("Fire2"))
                {
                    clickSignal.Dispatch(null);
                }
            }
        }

        void TestSelection()
        {
            if (raycastModel.worldHit.HasValue)
            {
                clickSignal.Dispatch(raycastModel.worldHit.Value.collider.gameObject);
            }
            else
            {
                clickSignal.Dispatch(null);
            }
        }

    }
}


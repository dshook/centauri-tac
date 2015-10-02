using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class TileClickView : View
    {
        internal Signal<GameObject> clickSignal = new Signal<GameObject>();

        bool active = false;

        internal void init()
        {
            active = true;
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
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                clickSignal.Dispatch(objectHit.collider.gameObject);
            }
            else
            {
                clickSignal.Dispatch(null);
            }
        }

    }
}


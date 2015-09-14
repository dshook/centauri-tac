using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ctac
{
    public class MinionSelectView : View
    {
        internal Signal<GameObject> minionSelected = new Signal<GameObject>();

        bool somethingSelected = false;

        void Update()
        {
            if (CrossPlatformInputManager.GetButton("Fire1"))
            {
                TestSelection();
            }
        }

        bool TestSelection()
        {
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit minionHit;
            if (Physics.Raycast(camRay, out minionHit, Constants.cameraRaycastDist))
            {
                if (minionHit.collider.gameObject.CompareTag("Minion"))
                {
                    somethingSelected = true;
                    minionSelected.Dispatch(minionHit.collider.gameObject);
                }
            }
            else
            {
                minionSelected.Dispatch(null);
                somethingSelected = false;
            }

            return false;
        }

    }
}


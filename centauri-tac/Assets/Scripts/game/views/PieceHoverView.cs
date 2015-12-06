using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class PieceHoverView : View
    {
        internal Signal<GameObject> pieceHover = new Signal<GameObject>();

        GameObject hoveredPiece = null;

        bool active = false;
        float rayFrequency = 0.1f;
        float timer = 0f;

        internal void init()
        {
            active = true;
        }

        void Update()
        {
            if (active)
            {
                timer += Time.deltaTime;
                if (timer > rayFrequency)
                {
                    timer = 0f;

                    //only mouse handling for now
                    CameraToMouseRay();
                }

            }
        }

        void CameraToMouseRay()
        {
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit pieceHit;
            if (Physics.Raycast(camRay, out pieceHit, Constants.cameraRaycastDist))
            {
                if (hoveredPiece != pieceHit.collider.gameObject && pieceHit.collider.gameObject.CompareTag("Piece"))
                {
                    hoveredPiece = pieceHit.collider.gameObject;
                    pieceHover.Dispatch(pieceHit.collider.gameObject);
                }
            }
            else
            {
                hoveredPiece = null;
                pieceHover.Dispatch(null);
            }
        }


    }
}


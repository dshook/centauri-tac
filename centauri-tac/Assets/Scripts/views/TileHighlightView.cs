using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using ctac.signals;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class TileHighlightView : View
    {
        internal Signal<GameObject> tileHover = new Signal<GameObject>();

        bool active = false;
        float rayFrequency = 0.1f;
        float timer = 0f;
        int tileMask = 0;

        GameObject hoveredTile = null;
        Color hoverColor = new Color(.9f, .9f, .9f);

        internal void init()
        {
            active = true;
            tileMask = LayerMask.GetMask("Tile");
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

            RaycastHit floorHit;
            if (Physics.Raycast(camRay, out floorHit, Constants.cameraRaycastDist, tileMask))
            {
                tileHover.Dispatch(floorHit.collider.gameObject);
            }
            else
            {
                tileHover.Dispatch(null);
            }
        }

        internal void onTileHover(GameObject newTile)
        {
            if (hoveredTile != null)
            {
                var spriteRenderer = hoveredTile.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = Color.white;
            }

            if (newTile != null)
            {
                hoveredTile = newTile;
                var spriteRenderer = hoveredTile.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = hoverColor;
            }
        }
    }
}


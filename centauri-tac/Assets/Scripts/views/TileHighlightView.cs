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
        internal Signal<GameObject> minionSelected = new Signal<GameObject>();

        GameObject hoveredTile = null;
        GameObject selectedTile = null;

        bool active = false;
        float rayFrequency = 0.1f;
        float timer = 0f;
        int tileMask = 0;

        Color hoverColor = new Color(.9f, .9f, .9f);
        Color selectColor = new Color(.4f, .9f, .4f);

        internal void init()
        {
            active = true;
            tileMask = LayerMask.GetMask("Tile");
        }

        void Update()
        {
            if (active)
            {
                if (CrossPlatformInputManager.GetButtonDown("Fire1"))
                {
                    TestSelection();
                }

                //right click et al deselects
                if (CrossPlatformInputManager.GetButtonDown("Fire2"))
                {
                    minionSelected.Dispatch(null);
                }

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
                if (selectedTile == null || (selectedTile != null && floorHit.collider.gameObject != selectedTile))
                {
                    tileHover.Dispatch(floorHit.collider.gameObject);
                }
            }
            else
            {
                tileHover.Dispatch(null);
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
                    minionSelected.Dispatch(minionHit.collider.gameObject);
                }
            }
            else
            {
                minionSelected.Dispatch(null);
            }

            return false;

        }

        internal void onTileHover(GameObject newTile)
        {
            if (hoveredTile != null && hoveredTile != selectedTile)
            {
                var spriteRenderer = hoveredTile.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = Color.white;
            }

            if (newTile != null && newTile != selectedTile)
            {
                hoveredTile = newTile;
                var spriteRenderer = hoveredTile.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = hoverColor;
            }
        }

        internal void onTileSelected(GameObject newTile)
        {
            if (selectedTile != null)
            {
                var spriteRenderer = selectedTile.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = Color.white;
            }

            if (newTile != null)
            {
                selectedTile = newTile;
                var spriteRenderer = selectedTile.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.color = selectColor;
            }
        }
    }
}


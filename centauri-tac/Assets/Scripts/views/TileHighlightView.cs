using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using ctac.signals;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections.Generic;

namespace ctac
{
    public class TileHighlightView : View
    {
        internal Signal<GameObject> tileHover = new Signal<GameObject>();
        internal Signal<GameObject> minionSelected = new Signal<GameObject>();

        Tile hoveredTile = null;
        Tile selectedTile = null;
        Dictionary<Vector2, Tile> moveTiles = null;

        bool active = false;
        float rayFrequency = 0.1f;
        float timer = 0f;
        int tileMask = 0;

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
                if (selectedTile == null || (selectedTile != null && floorHit.collider.gameObject != selectedTile.gameObject))
                {
                    tileHover.Dispatch(floorHit.collider.gameObject);
                }
            }
            else
            {
                tileHover.Dispatch(null);
            }
        }


        void TestSelection()
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
        }

        internal void onTileHover(Tile newTile)
        {
            if (hoveredTile != null && hoveredTile != selectedTile)
            {
                FlagsHelper.Unset(ref hoveredTile.highlightStatus, TileHighlightStatus.Highlighted);
            }

            if (newTile != null && newTile != selectedTile)
            {
                hoveredTile = newTile;
                FlagsHelper.Set(ref hoveredTile.highlightStatus, TileHighlightStatus.Highlighted);
            }
        }

        internal void onTileSelected(Tile newTile)
        {
            if (selectedTile != null)
            {
                FlagsHelper.Unset(ref selectedTile.highlightStatus, TileHighlightStatus.Selected);
            }

            selectedTile = newTile;
            if (selectedTile != null)
            {
                FlagsHelper.Set(ref selectedTile.highlightStatus, TileHighlightStatus.Selected);
            }
        }

        internal void onMovableTiles(Dictionary<Vector2, Tile> tiles)
        {
            if (moveTiles != null && moveTiles.Count > 0)
            {
                foreach (var tile in moveTiles)
                {
                    FlagsHelper.Unset(ref tile.Value.highlightStatus, TileHighlightStatus.Movable);
                }
            }

            moveTiles = tiles;
            if (tiles != null)
            {
                foreach (var tile in tiles)
                {
                    FlagsHelper.Set(ref tile.Value.highlightStatus, TileHighlightStatus.Movable);
                }
            }
        }
    }
}


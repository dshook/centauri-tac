using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using System.Collections.Generic;

namespace ctac
{
    public class TileHighlightView : View
    {
        internal Signal<GameObject> tileHover = new Signal<GameObject>();

        Tile hoveredTile = null;
        Tile selectedTile = null;
        Tile attackTile = null;

        internal void onTileHover(Tile newTile)
        {
            if (hoveredTile != null)
            {
                FlagsHelper.Unset(ref hoveredTile.highlightStatus, TileHighlightStatus.Highlighted);
            }

            if (newTile != null)
            {
                hoveredTile = newTile;
                FlagsHelper.Set(ref hoveredTile.highlightStatus, TileHighlightStatus.Highlighted);
            }
        }

        internal void onAttackTile(Tile newTile)
        {
            if (attackTile != null)
            {
                FlagsHelper.Unset(ref attackTile.highlightStatus, TileHighlightStatus.Attack);
            }

            attackTile = newTile;
            if (attackTile != null)
            {
                FlagsHelper.Set(ref attackTile.highlightStatus, TileHighlightStatus.Attack);
            }
        }

        internal void onTileSelected(Tile newTile)
        {
            if (selectedTile != null)
            {
                FlagsHelper.Unset(ref selectedTile.highlightStatus, TileHighlightStatus.Selected);
                selectedTile.showPieceRotation = false;
            }

            selectedTile = newTile;
            if (selectedTile != null)
            {
                FlagsHelper.Set(ref selectedTile.highlightStatus, TileHighlightStatus.Selected);
                selectedTile.showPieceRotation = true;
            }
        }

        private Dictionary<TileHighlightStatus, List<Tile>> tileStatuses = new Dictionary<TileHighlightStatus, List<Tile>>();
        internal void toggleTileFlags(List<Tile> tiles, TileHighlightStatus status)
        {
            List<Tile> toggledTiles;
            
            if (tileStatuses.TryGetValue(status, out toggledTiles) && toggledTiles != null && toggledTiles.Count > 0)
            {
                foreach (var tile in toggledTiles)
                {
                    FlagsHelper.Unset(ref tile.highlightStatus, status);
                }
            }

            tileStatuses[status] = tiles;
            if (tiles != null)
            {
                foreach (var tile in tiles)
                {
                    FlagsHelper.Set(ref tile.highlightStatus, status);
                }
            }
        }
    }
}


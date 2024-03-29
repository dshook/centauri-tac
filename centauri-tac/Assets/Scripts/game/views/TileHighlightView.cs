using strange.extensions.mediation.impl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class TileHighlightView : View
    {
        [Inject] public MapModel map { get; set; }

        Tile hoveredTile = null;
        Tile selectedTile = null;
        List<Tile> attackTiles = null;

        public GameObject tileCursor = null; 

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

                if (tileCursor != null)
                {
                    tileCursor.SetActive(true);
                    tileCursor.transform.position = newTile.fullPosition.AddY(0.01f);
                }
            }
            else if(tileCursor != null)
            {
                tileCursor.SetActive(false);
            }
        }

        internal void onAttackTile(List<Tile> newTiles)
        {
            if (attackTiles != null)
            {
                foreach (var tile in attackTiles)
                {
                    if(tile == null) continue;
                    FlagsHelper.Unset(ref tile.highlightStatus, TileHighlightStatus.Attack);
                    tile.pieceIndicatorView.SetStatus(false);
                }
            }

            attackTiles = newTiles;
            if (attackTiles != null)
            {
                foreach (var tile in attackTiles)
                {
                    if(tile == null) continue;
                    FlagsHelper.Set(ref tile.highlightStatus, TileHighlightStatus.Attack);
                    tile.pieceIndicatorView.SetStatus(true, true);
                }
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
        internal void toggleTileFlags(List<Tile> tiles, TileHighlightStatus status, bool dimTiles = false)
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

            //dim all tiles NOT in the tiles list
            if (dimTiles)
            {
                List<Tile> dimmedTiles;
                if (tileStatuses.TryGetValue(TileHighlightStatus.Dimmed, out dimmedTiles) && dimmedTiles != null && dimmedTiles.Count > 0)
                {
                    foreach (var tile in dimmedTiles)
                    {
                        FlagsHelper.Unset(ref tile.highlightStatus, TileHighlightStatus.Dimmed);
                    }
                }
                List<Tile> toDim;
                if (tiles == null)
                {
                    toDim = null;
                }
                else
                {
                    toDim = map.tileList.Except(tiles).ToList();
                }
                tileStatuses[TileHighlightStatus.Dimmed] = toDim;
                if (toDim != null)
                {
                    foreach (var tile in toDim)
                    {
                        FlagsHelper.Set(ref tile.highlightStatus, TileHighlightStatus.Dimmed);
                    }
                }
            }
        }
    }
}


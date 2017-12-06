using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    [Singleton]
    public class MapModel
    {
        public string name { get; set; }
        public int maxPlayers { get; set; }
        public GameObject root { get; set; }
        public Dictionary<Vector2, Tile> tiles { get; set; }
        public List<Tile> cosmeticTiles { get; set; }
        public List<PropView> props { get; set; }

        public List<Tile> tileList
        {
            get
            {
                return tiles.Select(x => x.Value).ToList();
            }
        }

        public List<Tile> getTilesByPosition(List<Vector2> positions)
        {
            return tiles.Where((k) => positions.Contains(k.Key)).Select(a => a.Value).ToList(); 
        }
    }

    public class Tile
    {
        public Vector2 position { get; set; }
        public Vector3 fullPosition { get; set; }
        public GameObject gameObject { get; set; }
        public TileHighlightStatus highlightStatus;
        public bool showPieceRotation { get; set; }
        public bool unpassable { get; set; }
        public GameObject indicator { get; set; }

        private TilePieceIndicatorialView _pieceIndicatorView;
        public TilePieceIndicatorialView pieceIndicatorView
        {
            get
            {
                if (_pieceIndicatorView == null && gameObject != null)
                {
                    _pieceIndicatorView = gameObject.GetComponentInChildren<TilePieceIndicatorialView>();
                }
                return _pieceIndicatorView;
            }
        }
    }

    [Flags]
    public enum TileHighlightStatus
    {
        //None              = (1 << 0),
        Highlighted       = (1 << 0), //hovering over the tile
        Selected          = (1 << 1), //deploying a piece to the tile
        MoveRange         = (1 << 2), //we can move to this tile
        PathFind          = (1 << 3), //tiles part of the found move path
        Attack            = (1 << 4), //there's an enemy piece we can actually attack
        AttackRange       = (1 << 5), //we can attack this tile
        TargetTile        = (1 << 6), //tiles part of a targeting area
        Dimmed            = (1 << 7)

    }
}


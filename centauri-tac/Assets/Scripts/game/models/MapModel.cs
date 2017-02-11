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
        Highlighted       = (1 << 0),
        Selected          = (1 << 1),
        Movable           = (1 << 2),
        PathFind          = (1 << 3),
        Attack            = (1 << 4),
        MoveRange         = (1 << 5),
        AttackRange       = (1 << 6),
        TargetTile        = (1 << 7),
        Dimmed            = (1 << 8)

    }
}


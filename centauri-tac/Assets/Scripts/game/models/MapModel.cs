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
    }

    [Flags]
    public enum TileHighlightStatus
    {
        None = 0,
        Highlighted = 1,
        Selected = 2,
        Movable = 4,
        PathFind = 8,
        Attack = 16,
        MoveRange = 32,
        AttackRange = 64,
        FriendlyTauntArea = 128,
        EnemyTauntArea = 256,
        TargetTile = 512
    }
}


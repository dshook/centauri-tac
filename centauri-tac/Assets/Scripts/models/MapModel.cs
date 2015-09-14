using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public interface IMapModel
    {
        string name { get; set; }
        int maxPlayers { get; set; }
        GameObject root { get; set; }
        Dictionary<Vector2, Tile> tiles { get; set; }
    }

    [Singleton]
    public class MapModel : IMapModel
    {
        public string name { get; set; }
        public int maxPlayers { get; set; }
        public GameObject root { get; set; }
        public Dictionary<Vector2, Tile> tiles { get; set; }
    }

    public class Tile
    {
        public GameObject gameObject { get; set; }
    }
}


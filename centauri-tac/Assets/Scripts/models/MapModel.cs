using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public interface IMapModel
    {
        string name { get; set; }
        int maxPlayers { get; set; }
        GameObject root { get; set; }
        List<Tile> tiles { get; set; }
    }

    public class MapModel : IMapModel
    {
        public string name { get; set; }
        public int maxPlayers { get; set; }
        public GameObject root { get; set; }
        public List<Tile> tiles { get; set; }
    }

    public class Tile
    {
        public GameObject gameObject { get; set; }
    }
}


using System.Collections.Generic;

namespace ctac
{
    public interface IMapModel
    {
        string name { get; set; }
        int maxPlayers { get; set; }
        List<Tile> tiles { get; set; }
    }

    public class MapModel : IMapModel
    {
        public string name { get; set; }
        public int maxPlayers { get; set; }
        public List<Tile> tiles { get; set; }
    }

    public class Tile
    {
        public TilePosition transform;
    }

    public class TilePosition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }
}


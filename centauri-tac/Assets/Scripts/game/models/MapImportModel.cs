using System.Collections.Generic;

namespace ctac
{
    public class MapImportModel
    {
        public string name { get; set; }
        public int maxPlayers { get; set; }
        public List<TileImport> tiles { get; set; }
    }

    public class TileImport
    {
        public TileImportPosition transform;
    }

    public class TileImportPosition
    {
        public int x { get; set; }
        public float y { get; set; }
        public int z { get; set; }
    }
}


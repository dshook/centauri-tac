using System.Collections.Generic;

namespace ctac
{
    public interface IMapImportModel
    {
        string name { get; set; }
        int maxPlayers { get; set; }
        List<TileImport> tiles { get; set; }
    }

    public class MapImportModel : IMapImportModel
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
        public int y { get; set; }
        public int z { get; set; }
    }
}


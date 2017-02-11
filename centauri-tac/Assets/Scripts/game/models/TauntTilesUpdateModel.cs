using System.Collections.Generic;

namespace ctac
{
    public class TauntTilesUpdateModel
    {
        public List<Tile> friendlyTauntTiles { get; set; }
        public List<Tile> friendlyTauntPieceTiles { get; set; }
        public List<Tile> enemyTauntTiles { get; set; }
        public List<Tile> enemyTauntPieceTiles { get; set; }
    }
}

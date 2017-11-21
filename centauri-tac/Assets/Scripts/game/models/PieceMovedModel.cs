using UnityEngine;

namespace ctac
{
    public class PieceMovedModel
    {
        public PieceModel piece { get; set; }
        public Tile to { get; set; }
        public Direction direction { get; set; }
        public bool isTeleport { get; set; }
    }
}

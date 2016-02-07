using System.Collections.Generic;

namespace ctac
{
    public class SpawnPieceModel
    {
        public int id { get; set; }
        public int? pieceId { get; set; }
        public int cardTemplateId { get; set; }
        public int playerId { get; set; }
        public PositionModel position { get; set; }

        public List<string> tags { get; set; }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class PiecesModel
    {
        public List<PieceModel> Pieces { get; set; }

        public PieceModel Piece(int id)
        {
            return Pieces.FirstOrDefault(x => x.id == id);
        }

        public PieceModel Hero(int playerId)
        {
            return Pieces.FirstOrDefault(x => x.playerId == playerId && x.tags.Contains("Hero"));
        }
    }
}

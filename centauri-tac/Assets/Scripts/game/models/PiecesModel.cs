using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    [GameSingleton]
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

        public PieceModel PieceAt(Vector2 tilePosition)
        {
            return Pieces.FirstOrDefault(x => x.tilePosition == tilePosition);
        }
    }
}

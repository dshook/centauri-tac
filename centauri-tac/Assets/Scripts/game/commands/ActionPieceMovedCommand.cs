using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceMovedCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public MovePieceModel movePiece { get; set; }

        [Inject]
        public PieceMovedSignal pieceMove { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(movePiece.id)) return;

            var piece = piecesModel.Pieces.FirstOrDefault(x => x.id == movePiece.pieceId);
            var toTile = map.tiles[movePiece.to.Vector3.ToTileCoordinates()];

            pieceMove.Dispatch(new PieceMovedModel()
            {
                piece = piece,
                to = toTile
            });

            debug.Log( string.Format("Moved piece {0} to {1}", movePiece.pieceId, movePiece.to) , socketKey );
        }
    }
}


using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionRotatePieceCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public RotatePieceModel rotatePiece { get; set; }

        [Inject]
        public PieceRotatedSignal pieceRotated { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(rotatePiece.id)) return;

            var piece = piecesModel.Pieces.FirstOrDefault(x => x.id == rotatePiece.pieceId);
            piece.direction = rotatePiece.direction;

            pieceRotated.Dispatch(rotatePiece);

            debug.Log( string.Format("Rotated piece {0} to {1}", rotatePiece.pieceId, rotatePiece.direction) , socketKey );
        }
    }
}


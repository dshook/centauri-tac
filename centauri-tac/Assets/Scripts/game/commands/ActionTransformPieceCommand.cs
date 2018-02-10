using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionTransformPieceCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public TransformPieceModel transformPiece { get; set; }

        [Inject]
        public PieceTransformedSignal pieceTransformed { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public IPieceService pieceService { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(transformPiece.id)) return;

            var piece = piecesModel.Pieces.FirstOrDefault(x => x.id == transformPiece.pieceId);

            transformPiece.oldStatuses = piece.statuses;
            pieceService.CopyPropertiesFromPiece(transformPiece.updatedPiece, piece);

            pieceTransformed.Dispatch(transformPiece);

            debug.Log( string.Format("Transformed piece {0} to {1}", transformPiece.pieceId, transformPiece.updatedPiece.name) , socketKey );
        }
    }
}


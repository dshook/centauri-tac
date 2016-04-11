using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceDestroyedCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PieceDestroyedModel pieceDestroyed { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceDestroyed.id)) return;

            var piece = pieces.Piece(pieceDestroyed.pieceId);

            pieceDied.Dispatch(piece);

            debug.Log( string.Format("Piece {0} Destroyed", pieceDestroyed.pieceId), socketKey );
        }
    }
}


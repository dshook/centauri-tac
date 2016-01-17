using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceHealthChangeCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PieceHealthChangeModel pieceChanged { get; set; }

        [Inject]
        public PieceHealthChangedSignal healthChanged { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceChanged.id)) return;

            pieces.Piece(pieceChanged.pieceId).health = pieceChanged.newCurrentHealth;

            healthChanged.Dispatch(pieceChanged);

            debug.Log( string.Format("Piece {0} {1} {2} health", pieceChanged.pieceId, (pieceChanged.change > 0 ? "gained" : "lost"), pieceChanged.change) , socketKey );
        }
    }
}


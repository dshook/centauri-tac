using strange.extensions.command.impl;
using ctac.signals;

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

            var piece = pieces.Piece(pieceChanged.pieceId);
            piece.health = pieceChanged.newCurrentHealth;

            pieceChanged.armorChange = pieceChanged.newCurrentArmor - piece.armor;
            piece.armor = pieceChanged.newCurrentArmor;

            healthChanged.Dispatch(pieceChanged);

            debug.Log( string.Format("Piece {0} {1} {2} health", pieceChanged.pieceId, (pieceChanged.change > 0 ? "gained" : "lost"), pieceChanged.change) , socketKey );
        }
    }
}


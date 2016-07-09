using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionPieceArmorChangeCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PieceArmorChangeModel pieceChanged { get; set; }

        [Inject]
        public PieceArmorChangedSignal healthChanged { get; set; }

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
            piece.armor = pieceChanged.newArmor;

            healthChanged.Dispatch(pieceChanged);

            debug.Log( string.Format("Piece {0} {1} {2} armor", pieceChanged.pieceId, (pieceChanged.change > 0 ? "gained" : "lost"), pieceChanged.change) , socketKey );
        }
    }
}


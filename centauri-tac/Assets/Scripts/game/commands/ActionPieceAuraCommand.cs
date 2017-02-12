using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionPieceAuraCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PieceAuraModel pieceAura { get; set; }

        [Inject]
        public PieceAuraSignal pieceAuraSignal { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceAura.id)) return;

            var piece = pieces.Piece(pieceAura.pieceId);

            piece.hasAura = true;
            pieceAuraSignal.Dispatch(pieceAura);

            debug.Log( string.Format("Piece {0} got aura with {1}", pieceAura.pieceId, pieceAura.name), socketKey );
        }
    }
}


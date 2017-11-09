using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionPieceAuraCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject] public PieceAuraModel pieceAura { get; set; }

        [Inject] public PieceAuraSignal pieceAuraSignal { get; set; }
        [Inject] public PieceStatusChangeSignal pieceStatusChange { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceAura.id)) return;

            var piece = pieces.Piece(pieceAura.pieceId);

            piece.hasAura = true;
            piece.statuses = piece.statuses | Statuses.hasAura;
            pieceAuraSignal.Dispatch(pieceAura);

            pieceStatusChange.Dispatch(new PieceStatusChangeModel()
            {
                pieceId = piece.id,
                add = Statuses.hasAura,
                statuses = piece.statuses
            });

            debug.Log( string.Format("Piece {0} got aura with {1}", pieceAura.pieceId, pieceAura.name), socketKey );
        }
    }
}


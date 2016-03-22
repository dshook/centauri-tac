using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceStatusChangeCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PieceStatusChangeModel pieceStatusChanged { get; set; }

        [Inject]
        public PieceStatusChangeSignal statusChangeSignal { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceStatusChanged.id)) return;

            var piece = pieces.Piece(pieceStatusChanged.pieceId);

            piece.statuses = pieceStatusChanged.statuses;

            piece.health = pieceStatusChanged.newHealth ?? piece.health;
            piece.attack = pieceStatusChanged.newAttack ?? piece.attack;
            piece.movement = pieceStatusChanged.newMovement ?? piece.movement;

            statusChangeSignal.Dispatch(pieceStatusChanged);

            debug.Log( 
                string.Format(
                    "Piece {0} added {1} statuses lost {2} result {3}"
                    , pieceStatusChanged.pieceId, pieceStatusChanged.add, pieceStatusChanged.remove, pieceStatusChanged.statuses
                )
                , socketKey 
            );
        }
    }
}


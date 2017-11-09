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

            //if silenced we have to also remove the event client statuses, don't think we need to remove the event ones though 
            //since they should get cleared from possible actions
            if (pieceStatusChanged.add.HasValue && FlagsHelper.IsSet(pieceStatusChanged.add.Value, Statuses.Silence))
            {
                Statuses removing = Statuses.None;
                if (piece.hasAura)
                {
                    FlagsHelper.Set(ref removing, Statuses.hasAura);
                    piece.hasAura = false;
                }

                pieceStatusChanged.remove = pieceStatusChanged.remove ?? Statuses.None;
                pieceStatusChanged.remove = pieceStatusChanged.remove | removing;

                var newStatuses = piece.statuses;
                FlagsHelper.Unset(ref newStatuses, removing);
                piece.statuses = newStatuses;
            }

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


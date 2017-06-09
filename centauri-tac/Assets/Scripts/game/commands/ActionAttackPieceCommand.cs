using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionAttackPieceCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public AttackPieceModel attackedPiece { get; set; }

        [Inject]
        public PieceAttackedSignal pieceAttacked { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(attackedPiece.id)) return;

            var attacker = pieces.Piece(attackedPiece.attackingPieceId);
            attacker.attackCount++;

            if (attacker.isRanged)
            {
                attacker.hasMoved = true;
            }

            pieceAttacked.Dispatch(attackedPiece);

            debug.Log( string.Format("Piece {0} Attacked {1}", attackedPiece.attackingPieceId, attackedPiece.targetPieceId) , socketKey );
        }
    }
}


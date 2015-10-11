using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class PieceAttackedCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public AttackPieceModel attackedPiece { get; set; }

        [Inject]
        public MinionAttackedSignal minionAttacked { get; set; }

        [Inject]
        public MinionsModel minions { get; set; }

        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            //check to see if this action has already been processed by another player
            if (processedActions.processedActions.Any(x => x == attackedPiece.id))
            {
                return;
            }
            processedActions.processedActions.Add(attackedPiece.id);

            var attacker = minions.Minion(attackedPiece.attackingPieceId);
            var target = minions.Minion(attackedPiece.targetPieceId);

            minionAttacked.Dispatch(attackedPiece);

            debug.Log( string.Format("Piece {0} Attacked {1}", attackedPiece.attackingPieceId, attackedPiece.targetPieceId) , socketKey );
        }
    }
}


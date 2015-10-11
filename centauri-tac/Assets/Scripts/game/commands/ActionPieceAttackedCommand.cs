using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceAttackedCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public AttackPieceModel attackedPiece { get; set; }

        [Inject]
        public PieceAttackedSignal pieceAttacked { get; set; }

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

            pieceAttacked.Dispatch(attackedPiece);

            debug.Log( string.Format("Piece {0} Attacked {1}", attackedPiece.attackingPieceId, attackedPiece.targetPieceId) , socketKey );
        }
    }
}


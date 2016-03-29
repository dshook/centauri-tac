using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceBuffCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PieceBuffModel pieceBuff { get; set; }

        [Inject]
        public PieceBuffSignal pieceBuffedSignal { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceBuff.id)) return;

            var piece = pieces.Piece(pieceBuff.pieceId);

            if (pieceBuff.removed)
            {
                var buff = piece.buffs.FirstOrDefault(x => x.name == pieceBuff.name);
                piece.buffs.Remove(buff);
            }
            else
            {
                piece.buffs.Add(pieceBuff);
            }

            piece.health = pieceBuff.newHealth ?? piece.health;
            piece.attack = pieceBuff.newAttack ?? piece.attack;
            piece.movement = pieceBuff.newMovement ?? piece.movement;

            pieceBuffedSignal.Dispatch(pieceBuff);

            debug.Log( string.Format("Piece {0} buffed with {1}", pieceBuff.pieceId, pieceBuff.name), socketKey );
        }
    }
}


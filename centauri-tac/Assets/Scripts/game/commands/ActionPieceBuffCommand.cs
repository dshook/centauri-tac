using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceBuffCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject] public PieceBuffModel pieceBuff { get; set; }

        [Inject] public PieceBuffSignal pieceBuffedSignal { get; set; }
        [Inject] public PieceStatusChangeSignal pieceStatusChange { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public IDebugService debug { get; set; }

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

            //some client side hackery here. If the buff changes range we have to fake a status change update to 
            //add or remove the range icon on the piece
            Statuses adding = Statuses.None;
            Statuses removing = Statuses.None;
            if (piece.range.HasValue && pieceBuff.newRange.HasValue && pieceBuff.newRange.Value == 0)
            {
                removing = Statuses.isRanged;
            }
            if ((!piece.range.HasValue || piece.range == 0) && pieceBuff.newRange.HasValue && pieceBuff.newRange > 0)
            {
                adding = Statuses.isRanged;
            }
            
            piece.range = pieceBuff.newRange ?? piece.range;

            if (adding != Statuses.None || removing != Statuses.None)
            {
                var newStatuses = piece.statuses;
                FlagsHelper.Set(ref newStatuses, adding);
                FlagsHelper.Unset(ref newStatuses, removing);
                piece.statuses = newStatuses;

                pieceStatusChange.Dispatch(new PieceStatusChangeModel()
                {
                    pieceId = piece.id,
                    add = adding,
                    remove = removing,
                    statuses = piece.statuses
                });
            }

            pieceBuffedSignal.Dispatch(pieceBuff);

            debug.Log( string.Format("Piece {0} buffed with {1}", pieceBuff.pieceId, pieceBuff.name), socketKey );
        }
    }
}


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

            var existingBuff = piece.buffs.FirstOrDefault(x => x.buffId == pieceBuff.buffId);
            if (pieceBuff.removed)
            {
                if(existingBuff == null){
                    debug.LogWarning("Buff not found to remove " + pieceBuff.buffId);
                }else{
                    piece.buffs.Remove(existingBuff);
                }
            }
            else
            {
                if(existingBuff == null){
                    piece.buffs.Add(pieceBuff);
                }else{
                    //must be a buff update
                    existingBuff.CopyBuff(pieceBuff);
                }
            }

            piece.health = pieceBuff.newHealth ?? piece.health;
            piece.attack = pieceBuff.newAttack ?? piece.attack;
            piece.movement = pieceBuff.newMovement ?? piece.movement;

            Statuses adding = Statuses.None;
            Statuses removing = Statuses.None;
            //some client side hackery here. If the buff changes range we have to fake a status change update to
            //add or remove the range icon on the piece, also have to check the client statuses, duplicated in piece status change
            piece.UpdateStatuses(pieceBuff.addStatus, pieceBuff.removeStatus, pieceBuff.statuses, pieceBuff.newRange, out adding, out removing);

            piece.range = pieceBuff.newRange ?? piece.range;

            if (adding != Statuses.None || removing != Statuses.None)
            {
                debug.Log(
                    string.Format(
                        "Piece buff for {0} added {1} statuses lost {2} result {3}"
                        , pieceBuff.pieceId, adding, removing, pieceBuff.statuses
                    )
                    , socketKey
                );
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


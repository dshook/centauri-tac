using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class CancelSelectTargetCommand : Command
    {
        [Inject] public CardModel targetingCard { get; set; }

        [Inject] public PieceDiedSignal pieceDied { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public AnimationQueueModel animationQueue { get; set; }

        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            debug.Log("Cancel Select Target Command");
            //find and cleanup the phantom piece start target spawned if existed, it won't for spells
            var phantomPiece = pieces.Pieces.FirstOrDefault(p => p.tags.Contains(Constants.targetPieceTag));

            if (phantomPiece != null)
            {
                animationQueue.Add(
                    new PieceView.UnsummonAnim()
                    {
                        piece = phantomPiece.pieceView,
                        pieceDied = pieceDied
                    }
                );
            }
        }
    }
}


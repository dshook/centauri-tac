using strange.extensions.command.impl;
using System.Linq;
using ctac.signals;

namespace ctac
{
    public class ServerQueueProcessEndCommand : Command
    {

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public AnimationQueueModel animationQueue { get; set; }
        [Inject] public PieceDiedSignal pieceDied { get; set; }

        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            debug.Log("Client QPC");

            //Just in case a weird situation happens when you're deployin and the turn ends but the phantom piece sticks around
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


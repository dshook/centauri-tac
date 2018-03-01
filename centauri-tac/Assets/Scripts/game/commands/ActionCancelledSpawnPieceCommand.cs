using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class ActionCancelledSpawnPieceCommand : Command
    {
        [Inject] public SpawnPieceModel pieceSpawnCancelled { get; set; }

        [Inject] public PieceDiedSignal pieceDied { get; set; }

        [Inject] public PiecesModel pieces { get; set; }

        [Inject] public CardsModel cards { get; set; }
        [Inject] public AnimationQueueModel animationQueue { get; set; }
        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceSpawnCancelled.id)) return;

            debug.Log("Action Cancelled Spawn Piece Command");
            //find and cleanup the phantom piece start target spawned
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

            //reactivate card if needed
            if (pieceSpawnCancelled.cardInstanceId.HasValue)
            {
                var card = cards.Card(pieceSpawnCancelled.cardInstanceId.Value);
                if (card != null)
                {
                    card.activated = false;
                }
            }
        }
    }
}


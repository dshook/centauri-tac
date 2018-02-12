using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class ActionCancelledActivateCardCommand : Command
    {
        [Inject] public ActivateCardModel activateCancelled { get; set; }

        [Inject] public PieceDiedSignal pieceDied { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public AnimationQueueModel animationQueue { get; set; }
        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(activateCancelled.id)) return;

            debug.Log("Action Cancelled Activate Card Command");
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

            //reset activated flag
            var card = cards.Card(activateCancelled.cardInstanceId);
            if (card != null)
            {
                card.activated = false;
            }
            else
            {
                debug.LogWarning("Couldn't find a card to reactivate on cancelled activate");
            }
        }
    }
}


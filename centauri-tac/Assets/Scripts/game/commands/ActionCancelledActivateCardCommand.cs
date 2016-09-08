using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class ActionCancelledActivateCardCommand : Command
    {
        [Inject]
        public ActivateCardModel activateCancelled { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            //find and cleanup the phantom piece start target spawned
            var phantomPiece = pieces.Pieces.FirstOrDefault(p => p.tags.Contains(Constants.targetPieceTag));

            if (phantomPiece != null)
            {
                pieceDied.Dispatch(phantomPiece);
            }

            //reset activated flag
            var card = cards.Cards.FirstOrDefault(c => c.id == activateCancelled.cardInstanceId);
            if (card != null)
            {
                debug.LogWarning("Couldn't find a card to reactivate on cancelled activate");
                card.activated = false;
            }
        }
    }
}


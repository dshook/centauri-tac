using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionActivateCardCommand : Command
    {
        [Inject] public ActivateCardModel cardActivated { get; set; }
        [Inject] public SocketKey socketKey { get; set; }

        [Inject] public CardActivatedSignal cardActivatedSignal { get; set; }

        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public CardsModel cards { get; set; }

        public override void Execute()
        {
            if(!processedActions.Verify(cardActivated.id)) return;
            
            var card = cards.Card(cardActivated.cardInstanceId);
            cardActivated.card = card;
            if(card != null){
                card.activated = true; //just in case, should already be set
            }

            cardActivatedSignal.Dispatch(cardActivated);
        }
    }
}


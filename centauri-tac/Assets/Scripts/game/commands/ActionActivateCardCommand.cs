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
        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public ICardService cardService { get; set; }

        public override void Execute()
        {
            if(!processedActions.Verify(cardActivated.id)) return;
            
            var card = cards.Card(cardActivated.cardInstanceId);
            cardActivated.card = card;
            if(card != null){
                card.activated = true; //just in case, should already be set

                //enemy cards that are activated need to be filled out with the info now that we have it
                if(card.cardTemplateId == 0 && cardActivated.cardTemplateId.HasValue){
                    cardService.CopyCard(cardDirectory.Card(cardActivated.cardTemplateId.Value), card);
                    card.playerId = cardActivated.playerId;
                }
            }

            cardActivatedSignal.Dispatch(cardActivated);
        }
    }
}


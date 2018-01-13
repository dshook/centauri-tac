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

            //Activate card no longer really does anything because the subsequent spawn piece or play spell commands could cancel
            //From invalid targets or anything else. So the activate card is in each of those commands, as well as cancelling in each of those
            
        }
    }
}


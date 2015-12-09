using strange.extensions.command.impl;
using System.Linq;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public class ActionActivateCardCommand : Command
    {
        [Inject]
        public DestroyCardSignal destroyCard { get; set; }

        [Inject]
        public ActivateCardModel cardActivated { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }
        
        public override void Execute()
        {
            if(!processedActions.Verify(cardActivated.id)) return;

            //don't think there's a good way to pass the reference to the instance of the card dragged between
            //client and server at the moment.  perhaps introducing a cardInstanceId is needed
            destroyCard.Dispatch(cardActivated.cardId);
        }
    }
}


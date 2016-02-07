using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionPlaySpellCommand : Command
    {
        [Inject]
        public DestroyCardSignal destroyCard { get; set; }

        [Inject]
        public PlaySpellModel cardActivated { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }
        
        public override void Execute()
        {
            if(!processedActions.Verify(cardActivated.id)) return;

            if (cardActivated.cardInstanceId.HasValue)
            {
                destroyCard.Dispatch(cardActivated.cardInstanceId.Value);
            }
        }
    }
}


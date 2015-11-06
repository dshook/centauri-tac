using strange.extensions.command.impl;
using System.Linq;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public class ActionSetPlayerResourceCommand : Command
    {
        [Inject]
        public SetPlayerResourceModel setPlayerResource { get; set; }

        [Inject]
        public PlayerResourcesModel playerResource { get; set; }

        [Inject]
        public PlayerResourceSetSignal playerResourceSet { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }
        
        public override void Execute()
        {
            //check to see if this action has already been processed by another player
            if (processedActions.processedActions.Any(x => x == setPlayerResource.id))
            {
                return;
            }
            processedActions.processedActions.Add(setPlayerResource.id);

            playerResource.resources[setPlayerResource.playerId] = setPlayerResource.amount;

            playerResourceSet.Dispatch(setPlayerResource);
        }
    }
}


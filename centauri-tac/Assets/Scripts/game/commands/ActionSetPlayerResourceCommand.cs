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
            if (!processedActions.Verify(setPlayerResource.id)) return;

            playerResource.resources[setPlayerResource.playerId] = setPlayerResource.newAmount;
            playerResource.maxResources[setPlayerResource.playerId] = setPlayerResource.newMax;

            playerResourceSet.Dispatch(setPlayerResource);
        }
    }
}


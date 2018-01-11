using System;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [GameSingleton]
    public class ActionsProcessedModel
    {
        public List<int> processedActions = new List<int>();

        public bool Verify(int actionId)
        {
            //check to see if this action has already been processed by another player
            if (processedActions.Any(x => x == actionId))
            {
                return false;
            }

            processedActions.Add(actionId);
            return true;
        }
    }
}

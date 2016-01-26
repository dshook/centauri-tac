using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class PossibleActionsModel
    {
        public Dictionary<int, List<ActionTarget>> possibleTargets;

        public PossibleActionsModel()
        {
            possibleTargets = new Dictionary<int, List<ActionTarget>>();
        }

        public void Update(PossibleActions newActions)
        {
            possibleTargets[newActions.playerId] = newActions.targets;
        }
    }

    public class PossibleActions
    {
        public int playerId { get; set; }
        public List<ActionTarget> targets { get; set; }
    }

    public class ActionTarget
    {
        public int cardId { get; set; }
        public string @event { get; set; }
        public List<int> targetPieceIds { get; set; }
    }
}

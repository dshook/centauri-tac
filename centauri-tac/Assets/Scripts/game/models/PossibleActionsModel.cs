using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class PossibleActionsModel
    {
        public Dictionary<int, List<ActionTarget>> possibleActions;

        public PossibleActionsModel()
        {
            possibleActions = new Dictionary<int, List<ActionTarget>>();
        }

        public void Update(PossibleActions newActions)
        {
            possibleActions[newActions.playerId] = newActions.targets;
        }

        /// <summary>
        /// Gets any action targets if they exist for a card, null if none
        /// Note, that this may need to be scoped to the event but for now playMinion or playSpell will work the same
        /// </summary>
        public ActionTarget GetForCard(int playerId, int cardId)
        {
            return possibleActions[playerId].FirstOrDefault(x => x.cardId == cardId);
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

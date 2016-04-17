using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class PossibleActionsModel
    {
        public Dictionary<int, List<ActionTarget>> possibleActions;
        public Dictionary<int, List<AbilityTarget>> possibleAbilities;

        public PossibleActionsModel()
        {
            possibleActions = new Dictionary<int, List<ActionTarget>>();
            possibleAbilities = new Dictionary<int, List<AbilityTarget>>();
        }

        public void Update(PossibleActions newActions)
        {
            possibleActions[newActions.playerId] = newActions.targets;
            possibleAbilities[newActions.playerId] = newActions.abilities;
        }

        /// <summary>
        /// Gets any action targets if they exist for a card, null if none
        /// Note, that this may need to be scoped to the event but for now playMinion or playSpell will work the same
        /// </summary>
        public ActionTarget GetForCard(int playerId, int cardId)
        {
            if (!possibleActions.ContainsKey(playerId)){ return null; }

            return possibleActions[playerId].FirstOrDefault(x => x.cardId == cardId);
        }
    }

    public class PossibleActions
    {
        public int playerId { get; set; }
        public List<ActionTarget> targets { get; set; }
        public List<AbilityTarget> abilities { get; set; }
    }

    public class ActionTarget
    {
        public int cardId { get; set; }
        public string @event { get; set; }
        public List<int> targetPieceIds { get; set; }
    }

    public class AbilityTarget
    {
        public int pieceId { get; set; }
        public int abilityCost { get; set; }
        public int abilityChargeTime { get; set; }
        public int abilityCooldown { get; set; }
        public string ability { get; set; }
        public List<int> targetPieceIds { get; set; }
    }
}

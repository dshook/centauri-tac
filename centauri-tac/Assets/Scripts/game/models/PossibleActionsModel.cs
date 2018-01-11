using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    //ideally would be game singleton but hover card mediator needs to know about spell damage
    [Singleton]
    public class PossibleActionsModel
    {
        public Dictionary<int, List<ActionTarget>> possibleActions = new Dictionary<int, List<ActionTarget>>();
        public Dictionary<int, List<AbilityTarget>> possibleAbilities = new Dictionary<int, List<AbilityTarget>>();
        public Dictionary<int, List<AreaTarget>> possibleAreas = new Dictionary<int, List<AreaTarget>>();
        public Dictionary<int, List<MetCondition>> metConditions = new Dictionary<int, List<MetCondition>>();
        public List<EventedPiece> eventedPieces = new List<EventedPiece>();
        public Dictionary<int, List<ChoiceCard>> chooseCards = new Dictionary<int, List<ChoiceCard>>();
        public Dictionary<int, int> spellDamage = new Dictionary<int, int>();

        public void Update(PossibleActions newActions)
        {
            possibleActions[newActions.playerId] = newActions.targets;
            possibleAbilities[newActions.playerId] = newActions.abilities;
            possibleAreas[newActions.playerId] = newActions.areas;
            eventedPieces = newActions.eventedPieces;
            metConditions[newActions.playerId] = newActions.metConditions;
            chooseCards[newActions.playerId] = newActions.chooseCards;
            spellDamage[newActions.playerId] = newActions.spellDamage;
        }

        /// <summary>
        /// Gets any action targets if they exist for a card, null if none
        /// Note, that this may need to be scoped to the event but for now playMinion or playSpell will work the same
        /// </summary>
        public ActionTarget GetActionsForCard(int playerId, int cardId)
        {
            if (!possibleActions.ContainsKey(playerId)){ return null; }

            return possibleActions[playerId].FirstOrDefault(x => x.cardId == cardId);
        }

        /// <summary>
        /// Gets any ability targets if they exist for a piece, null if none
        /// </summary>
        public AbilityTarget GetAbilitiesForPiece(int playerId, int pieceId)
        {
            if (!possibleAbilities.ContainsKey(playerId)){ return null; }

            return possibleAbilities[playerId].FirstOrDefault(x => x.pieceId == pieceId);
        }

        public AreaTarget GetAreasForCard(int playerId, int cardId)
        {
            if (!possibleAreas.ContainsKey(playerId)){ return null; }

            return possibleAreas[playerId].FirstOrDefault(x => x.cardId == cardId);
        }

        public ChoiceCard GetChoiceCards(int playerId, int cardId)
        {
            if (!chooseCards.ContainsKey(playerId)){ return null; }

            return chooseCards[playerId].FirstOrDefault(x => x.cardId == cardId);
        }

        public int GetSpellDamage(int playerId)
        {
            if(!spellDamage.ContainsKey(playerId)) return 0;
            
            return spellDamage[playerId];
        }
    }

    public class PossibleActions
    {
        public int playerId { get; set; }
        public int spellDamage { get; set; }
        public List<ActionTarget> targets { get; set; }
        public List<AbilityTarget> abilities { get; set; }
        public List<AreaTarget> areas { get; set; }
        public List<EventedPiece> eventedPieces { get; set; }
        public List<MetCondition> metConditions { get; set; }
        public List<ChoiceCard> chooseCards { get; set; }
    }

    public class ActionTarget
    {
        public int? cardId { get; set; }
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

    public class AreaTarget
    {
        public int cardId { get; set; }
        public string @event { get; set; }
        public AreaType areaType { get; set; } 
        public int size { get; set; }
        public bool isCursor { get; set; }
        public bool isDoubleCursor { get; set; }
        public bool? bothDirections { get; set; }
        public bool selfCentered { get; set; }
        public bool stationaryArea { get; set; }
        public PositionModel centerPosition { get; set; }
        public PositionModel pivotPosition { get; set; }
        public List<PositionModel> areaTiles { get; set; }
    }

    public class EventedPiece
    {
        public int pieceId { get; set; }
        public string @event { get; set; }
    }

    public class MetCondition
    {
        public int cardId { get; set; }
    }

    public class ChoiceCard
    {
        public int cardId { get; set; }
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public int cardTemplateId { get; set; }
        public ActionTarget targets { get; set; }
    }

    public enum AreaType
    {
        Cross = 1,
        Square = 2,
        Line = 3,
        Row = 4,
        Diagonal = 5,
        PiecePosition = 6,
        Diamond = 7
    }
}

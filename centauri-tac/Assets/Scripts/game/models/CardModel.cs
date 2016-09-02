using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public class CardModel
    {
        public int id { get; set; } //id for instance of card, should be unique across all cards in decks/hands
        public int cardTemplateId { get; set; } //id for template card
        public int playerId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int cost { get; set; }
        public int baseCost { get; set; }
        public int attack { get; set; }
        public int health { get; set; }
        public int movement { get; set; }
        public int? range { get; set; }

        public List<string> tags { get; set; }
        public Statuses statuses { get; set; }

        public List<CardBuffModel> buffs { get; set; }


        [JsonIgnore]
        public GameObject gameObject { get; set; }
        [JsonIgnore]
        public RectTransform rectTransform { get; set; }
        [JsonIgnore]
        public CardView cardView { get; set; }

        //If this card has a condition, and it is met
        [JsonIgnore]
        public bool metCondition { get; set; }

        //If this card is representing an in play piece
        [JsonIgnore]
        public PieceModel linkedPiece { get; set; }

        [JsonIgnore]
        public bool isMinion
        {
            get
            {
                return tags.Contains("Minion");
            }
        }

        [JsonIgnore]
        public bool isHero
        {
            get
            {
                return tags.Contains("Hero");
            }
        }

        [JsonIgnore]
        public bool isSpell
        {
            get
            {
                return tags.Contains("Spell");
            }
        }

        /// <summary>
        /// Returns if this card needs to have some sort of targeting, area or targets
        /// </summary>
        public bool needsTargeting(PossibleActionsModel possibleActions)
        {
            var targets = possibleActions.GetActionsForCard(playerId, id);
            var area = possibleActions.GetAreasForCard(playerId, id);
            return (targets != null && targets.targetPieceIds.Count >= 1) || (area != null && area.isCursor);
        }

        public bool isChoose(PossibleActionsModel possibleActions)
        {
            return possibleActions.GetChoiceCards(playerId, id) != null;
        }

        //have enough mana?
        public bool playable { get; set; }
        
        //when the card has been activated and should be going through animation
        public bool activated { get; set; }

        //moves the card from being under deck control to in play under card canvas
        public void SetCardInPlay(GameObject contextView)
        {
            var cardParent = contextView.transform.FindChild(Constants.cardCanvas);
            gameObject.transform.SetParent(cardParent.transform);
        }

        public void SetupGameObject(GameObject cardGameObject)
        {
            gameObject = cardGameObject;
            rectTransform = cardGameObject.GetComponent<RectTransform>();
            var cardView = cardGameObject.AddComponent<CardView>();
            cardView.card = this;
            this.cardView = cardView;
        }
    }
}

using ctac.util;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public interface ICardService
    {
        CardModel CreateCard(CardModel cardModel, Transform parent, Vector3? spawnPosition = null);
        void SetupGameObject(CardModel model, GameObject cardGameObject);
        void UpdateCardArt(CardModel model);
        void CopyCard(CardModel src, CardModel dest);
        ActivateCardModel ActivateCardInstance(CardsModel cards, CardDirectory cardDirectory, int cardInstanceId, int cardTemplateId, int? spellDamage);
    }

    public class CardService : ICardService
    {
        [Inject] public IDebugService debug { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        public CardModel CreateCard(CardModel cardModel, Transform parent, Vector3? spawnPosition = null)
        {
            var cardPrefab = loader.Load<GameObject>("Card");

            var newCard = GameObject.Instantiate(
                cardPrefab,
                spawnPosition ?? Constants.cardSpawnPosition,
                Quaternion.identity
            ) as GameObject;
            if (parent != null)
            {
                newCard.transform.SetParent(parent, false);
            }

            cardModel.gameObject = newCard;
            cardModel.rectTransform = newCard.GetComponent<RectTransform>();

            UpdateCardName(cardModel);

            if (cardModel.cardTemplateId != 0)
            {
                UpdateCardArt(cardModel);
            }

            return cardModel;
        }

        public void SetupGameObject(CardModel model, GameObject cardGameObject)
        {
            model.gameObject = cardGameObject;
            model.rectTransform = cardGameObject.GetComponent<RectTransform>();
            var cardView = cardGameObject.AddComponent<CardView>();
            cardView.card = model;
            model.cardView = cardView;

            UpdateCardArt(model);
            UpdateCardName(model);
        }

        public void UpdateCardArt(CardModel model)
        {
            var render = loader.Load<Texture>("Models/" + model.cardTemplateId + "/render");
            var displayWrapper = model.gameObject.transform.Find("DisplayWrapper").gameObject;

            var front = displayWrapper.transform.Find("Front").gameObject;
            var frontRenderer = front.GetComponent<MeshRenderer>();

            var cardImagePath = string.Format("Images/cards/common_{0}", model.race.ToString().ToLower());
            var cardFront = loader.Load<Texture>(cardImagePath);
            if (cardFront != null)
            {
                frontRenderer.material.SetTexture("_MainTex", cardFront);
            }

            switch (model.rarity) {
                case Rarities.Free:
                    frontRenderer.material.SetColor("_RarityColor", Colors.RarityFree);
                    break;
                case Rarities.Common:
                    frontRenderer.material.SetColor("_RarityColor", Colors.RarityCommon);
                    break;
                case Rarities.Rare:
                    frontRenderer.material.SetColor("_RarityColor", Colors.RarityRare);
                    break;
                case Rarities.Exotic:
                    frontRenderer.material.SetColor("_RarityColor", Colors.RarityExotic);
                    break;
                case Rarities.Ascendant:
                    frontRenderer.material.SetColor("_RarityColor", Colors.RarityAscendant);
                    break;
            }

            var art = displayWrapper.transform.Find("Art").gameObject;
            var artRenderer = art.GetComponent<MeshRenderer>();
            if (render != null)
            {
                artRenderer.material.SetTexture("_MainTex", render);
            }
            else
            {
                artRenderer.material.SetTexture("_MainTex", null);
            }
        }

        public void CopyCard(CardModel src, CardModel dest){

            dest.cardTemplateId = src.cardTemplateId;
            dest.playerId = src.playerId;
            dest.name = src.name;
            dest.description = src.description;
            dest.cost = src.cost;
            dest.baseCost = src.baseCost;
            dest.attack = src.attack;
            dest.health = src.health;
            dest.movement = src.movement;
            dest.range = src.range;
            dest.tags = src.tags ?? new List<string>();
            dest.statuses = src.statuses;
            dest.rarity = src.rarity;
            dest.race = src.race;
            dest.buffs = src.buffs ?? new List<CardBuffModel>();
            dest.cardSet = src.cardSet;
            dest.playable = src.playable;

            UpdateCardName(dest);
        }

        public void UpdateCardName(CardModel card)
        {
            if (card == null || card.gameObject == null) return;

            card.gameObject.name = string.Format("Player {0} Card {1} Template {2}", card.playerId, card.id, card.cardTemplateId);
        }

        //Sets up the local copy of an activated card from spawn piece or play spell and returns the activate card model
        //that needs to be dispatched
        public ActivateCardModel ActivateCardInstance(CardsModel cards, CardDirectory cardDirectory, int cardInstanceId, int cardTemplateId, int? spellDamage)
        {
            var card = cards.Card(cardInstanceId);
            if(card == null){ return null; }

            var cardActivated = new ActivateCardModel(){
                playerId = card.playerId,
                cardInstanceId = cardInstanceId,
                cardTemplateId = cardTemplateId,
                spellDamage = spellDamage,
                card = card
            };

            card.activated = true; //just in case, should already be set

            //enemy cards that are activated need to be filled out with the info now that we have it
            if(card.cardTemplateId == 0 && cardActivated.cardTemplateId.HasValue){
                CopyCard(cardDirectory.Card(cardActivated.cardTemplateId.Value), card);
                card.playerId = cardActivated.playerId;
            }
            return cardActivated;
        }
    }
}


using ctac.util;
using UnityEngine;

namespace ctac
{
    public interface ICardService
    {
        CardModel CreateCard(CardModel cardModel, Transform parent, Vector3? spawnPosition = null);
        void SetupGameObject(CardModel model, GameObject cardGameObject);
        void UpdateCardArt(CardModel model);
    }

    public class CardService : ICardService
    {
        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public IResourceLoaderService loader { get; set; }

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
            newCard.name = "Player " + cardModel.playerId + " Card " + cardModel.id;

            cardModel.gameObject = newCard;
            cardModel.rectTransform = newCard.GetComponent<RectTransform>();

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
        }

        public void UpdateCardArt(CardModel model)
        {
            var render = loader.Load<Texture>("Models/" + model.cardTemplateId + "/render");
            var displayWrapper = model.gameObject.transform.FindChild("DisplayWrapper").gameObject;

            var front = displayWrapper.transform.FindChild("Front").gameObject;
            var frontRenderer = front.GetComponent<MeshRenderer>();

            var cardImagePath = string.Format("Images/cards/common_{0}", model.race.ToString().ToLower());
            var cardFront = loader.Load<Texture>(cardImagePath);
            if (cardFront != null)
            {
                frontRenderer.material.SetTexture("_MainTex", cardFront);
            }

            switch (model.rarity) {
                case Rarities.Common:
                    frontRenderer.material.SetColor("_RarityColor", ColorExtensions.HexToColor("999999"));
                    break;
                case Rarities.Rare:
                    frontRenderer.material.SetColor("_RarityColor", ColorExtensions.HexToColor("2598C5"));
                    break;
                case Rarities.Exotic:
                    frontRenderer.material.SetColor("_RarityColor", ColorExtensions.HexToColor("64FF07"));
                    break;
                case Rarities.Mythical:
                    frontRenderer.material.SetColor("_RarityColor", ColorExtensions.HexToColor("FF0000"));
                    break;
            }

            var art = displayWrapper.transform.FindChild("Art").gameObject;
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
    }
}


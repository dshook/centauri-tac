using UnityEngine;

namespace ctac
{
    public interface ICardService
    {
        CardModel CreateCard(CardModel cardModel, Transform parent, Vector3? spawnPosition = null);

        void SetupGameObject(CardModel model, GameObject cardGameObject);
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
                //set there is art to setup
                var render = loader.Load<Texture>("Models/" + cardModel.cardTemplateId + "/render");
                if (render != null)
                {
                    var art = newCard.transform.FindChild("Art").gameObject;
                    var artRenderer = art.GetComponent<MeshRenderer>();
                    artRenderer.material.SetTexture("_MainTex", render);
                }
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

            //set there is art to setup
            var render = loader.Load<Texture>("Models/" + model.cardTemplateId + "/render");
            if (render != null)
            {
                var displayWrapper = cardGameObject.transform.FindChild("DisplayWrapper").gameObject;
                var art = displayWrapper.transform.FindChild("Art").gameObject;
                var artRenderer = art.GetComponent<MeshRenderer>();
                artRenderer.material.SetTexture("_MainTex", render);
            }
        }
    }
}


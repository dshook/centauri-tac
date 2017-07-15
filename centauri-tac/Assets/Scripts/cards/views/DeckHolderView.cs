using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;
using System.Linq;

namespace ctac
{
    public class DeckHolderView : View
    {
        public Signal clickLeaveSignal = new Signal();

        GameObject miniCardsHolder;
        IResourceLoaderService loader;

        List<MiniCardView> cardList = new List<MiniCardView>();

        internal void init(IResourceLoaderService l)
        {
            loader = l;
            miniCardsHolder = transform.FindChild("MiniCards").gameObject;

            miniCardsHolder.transform.DestroyChildren(true);
        }

        void Update()
        {
        }

        internal void addCard(CardModel card)
        {
            cardList.Add(CreateMiniCard(card, miniCardsHolder.transform, new Vector3(-87.8f, 198 - (25 * cardList.Count))));
        }

        public MiniCardView CreateMiniCard(CardModel cardModel, Transform parent, Vector3? spawnPosition = null)
        {
            var cardPrefab = loader.Load<GameObject>("MiniCard");

            var newCard = GameObject.Instantiate(
                cardPrefab,
                spawnPosition ?? Constants.cardSpawnPosition,
                Quaternion.identity
            ) as GameObject;
            if (parent != null)
            {
                newCard.transform.SetParent(parent, false);
            }
            newCard.name = "Template " + cardModel.cardTemplateId + " Card " + cardModel.id;

            //copy to new card model so we won't affect the actual card display
            var miniCardModel = new CardModel();
            ObjectPropertyCopy.CopyProperties(cardModel, miniCardModel);
            miniCardModel.cardView = null;

            miniCardModel.gameObject = newCard;
            miniCardModel.rectTransform = newCard.GetComponent<RectTransform>();

            var miniCardView = newCard.AddComponent<MiniCardView>();
            miniCardView.card = miniCardModel;

            return miniCardView;
        }


    }
}


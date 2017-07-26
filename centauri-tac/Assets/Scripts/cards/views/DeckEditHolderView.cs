using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using UnityEngine;
using System.Linq;
using TMPro;

namespace ctac
{
    public class DeckEditHolderView : View
    {
        public GameObject miniCardsHolder;
        public GameObject deckName;

        RectTransform holderRectTransform;
        RectTransform scrollRectTransform;
        TMP_InputField deckNameText;

        IResourceLoaderService loader;

        DeckModel deck;
        List<MiniCardView> cardList = new List<MiniCardView>();

        internal void init(IResourceLoaderService l)
        {
            loader = l;
            holderRectTransform = miniCardsHolder.GetComponent<RectTransform>();

            var scrollRect = GetComponentInChildren<ScrollRect>();
            scrollRectTransform = scrollRect.gameObject.GetComponent<RectTransform>();

            deckNameText = deckName.GetComponent<TMP_InputField>();
            deckNameText.onValueChanged.AddListener(DeckNameChange);

            miniCardsHolder.transform.DestroyChildren(true);
            UpdateList();
        }

        void Update()
        {
        }

        internal void EditDeck(DeckModel editingDeck)
        {
            //if we're editing the same deck we should be good on resetting
            if (editingDeck == deck) return;

            //blow everything away
            cardList.Clear();
            miniCardsHolder.transform.DestroyChildren(true);

            deck = editingDeck;
            deckNameText.text = deck.name;

            foreach (var card in deck.Cards)
            {
                addCard(card);
            }
        }

        internal void DeckNameChange(string value)
        {
            deck.name = value;
        }

        internal void addCard(CardModel card)
        {
            //first check to see if we already have this card in the deck and thus we can just increase the quantity
            var foundCard = cardList.FirstOrDefault(c => c.card.cardTemplateId == card.cardTemplateId);

            if (foundCard != null)
            {
                if (foundCard.quantity == 2 || foundCard.card.rarity == Rarities.Mythical)
                {
                    //message pops up
                    return;
                }
                foundCard.quantity++;
                foundCard.UpdateText();
            }
            else
            {
                cardList.Add(CreateMiniCard(card, miniCardsHolder.transform));
                UpdateList();
            }
        }

        internal void removeCard(CardModel card)
        {
            var foundCard = cardList.FirstOrDefault(c => c.card.cardTemplateId == card.cardTemplateId);
            if (foundCard == null) return;

            if (foundCard.quantity > 1)
            {
                foundCard.quantity--;
                foundCard.UpdateText();
            }
            else
            {
                cardList.Remove(foundCard);
                Destroy(foundCard.card.gameObject);
                UpdateList();
            }
        }

        //sort and reposition list by cost
        void UpdateList()
        {
            cardList = cardList.OrderBy(c => c.card.cost).ToList();
            const float cardHeight = 25;
            const float padding = 5f;
            var contentHeight = cardHeight * cardList.Count + padding;
            contentHeight = Mathf.Max(contentHeight, scrollRectTransform.sizeDelta.y);
            holderRectTransform.sizeDelta = new Vector2(holderRectTransform.sizeDelta.x, contentHeight);

            for(int i = 0; i < cardList.Count; i++)
            {
                cardList[i].gameObject.transform.localPosition = new Vector3(81f, -15 - (cardHeight * i));
            }
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
            miniCardView.quantity = 1;

            return miniCardView;
        }


    }
}


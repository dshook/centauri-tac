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
        public TextMeshProUGUI deckCounter;

        RectTransform holderRectTransform;
        RectTransform scrollRectTransform;
        TMP_InputField deckNameText;

        IResourceLoaderService loader;
        CardDirectory directory;

        DeckModel deck;
        List<MiniCardView> cardList = new List<MiniCardView>();

        internal void init(IResourceLoaderService l, CardDirectory d)
        {
            loader = l;
            directory = d;
            holderRectTransform = miniCardsHolder.GetComponent<RectTransform>();

            var scrollRect = GetComponentInChildren<ScrollRect>();
            scrollRectTransform = scrollRect.gameObject.GetComponent<RectTransform>();

            deckNameText = deckName.GetComponent<TMP_InputField>();
            deckNameText.onValueChanged.AddListener(DeckNameChange);

            miniCardsHolder.transform.DestroyChildren(true);
            UpdateList();
            UpdateCounter();
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
            if (deck != null)
            {
                deckNameText.text = deck.name;

                foreach (var card in deck.cards)
                {
                    addCard(card.cardTemplateId, card.quantity);
                }
            }
            UpdateCounter();
        }

        //Convert all our card views
        internal void SaveDeck(DeckModel savingDeck)
        {
            savingDeck.cards = cardList
                .Select(c => new CardInDeckModel() { cardTemplateId = c.card.cardTemplateId, quantity = c.quantity } )
                .ToList();
        }

        internal void DeckNameChange(string value)
        {
            deck.name = value;
        }

        internal void addCard(int cardTemplateId, int quantity = 1)
        {
            //first check to see if we already have this card in the deck and thus we can just increase the quantity
            var foundCard = cardList.FirstOrDefault(c => c.card.cardTemplateId == cardTemplateId);

            if (foundCard != null)
            {
                if (foundCard.quantity == 2 || foundCard.quantity + quantity > 2 || foundCard.card.rarity == Rarities.Mythical)
                {
                    //TODO: message pops up
                    return;
                }
                foundCard.quantity++;
                foundCard.UpdateText();
                UpdateCounter();
            }
            else if (quantity > 2) {
                //TODO: message also pops up
            }
            else
            {
                cardList.Add(CreateMiniCard(cardTemplateId, miniCardsHolder.transform, quantity));
                UpdateList();
                UpdateCounter();
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
            UpdateCounter();
        }

        //sort and reposition list by cost
        void UpdateList()
        {
            cardList = cardList.OrderBy(c => c.card.cost).ToList();
            const float cardHeight = 25;
            const float margin = 5f;
            var contentHeight = cardHeight * cardList.Count + margin;
            contentHeight = Mathf.Max(contentHeight, scrollRectTransform.sizeDelta.y);
            holderRectTransform.sizeDelta = new Vector2(holderRectTransform.sizeDelta.x, contentHeight);

            for(int i = 0; i < cardList.Count; i++)
            {
                cardList[i].gameObject.transform.localPosition = new Vector3(81f, -15 - (cardHeight * i));
            }
        }

        void UpdateCounter()
        {
            deckCounter.text = string.Format("{0}/{1}", CardCount(), 30);
        }

        public MiniCardView CreateMiniCard(int cardTemplateId, Transform parent, int quantity)
        {
            var cardPrefab = loader.Load<GameObject>("MiniCard");

            var newCard = GameObject.Instantiate(
                cardPrefab,
                Constants.cardSpawnPosition,
                Quaternion.identity
            ) as GameObject;
            if (parent != null)
            {
                newCard.transform.SetParent(parent, false);
            }
            newCard.name = "Card Template " + cardTemplateId;

            var miniCardModel = directory.NewFromTemplate(0, cardTemplateId, 0);

            miniCardModel.gameObject = newCard;
            miniCardModel.rectTransform = newCard.GetComponent<RectTransform>();

            var miniCardView = newCard.AddComponent<MiniCardView>();
            miniCardView.card = miniCardModel;
            miniCardView.quantity = 1;

            return miniCardView;
        }

        int CardCount()
        {
            return (cardList == null || cardList.Count == 0) ? 0 
                : cardList.Sum(c => c.quantity);
        }

    }
}


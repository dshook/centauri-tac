using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;
using System.Linq;

namespace ctac
{
    public class CardsMenuView : View
    {
        public Signal clickLeaveSignal = new Signal();
        public Button leaveButton;

        ICardService cardService;
        CardDirectory cardDirectory;

        GameObject cardHolder;
        
        const int pageSize = 8;
        const int rowSize = 4;
        const int cardBufferSize = pageSize * 2;
        Vector2 cardAnchor = new Vector2(0, 1);

        List<CardModel> createdCards = new List<CardModel>();

        int offset = 0;

        internal void init(ICardService cs, CardDirectory cd)
        {
            cardService = cs;
            cardDirectory = cd;

            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
            cardHolder = GameObject.Find("CardHolder").gameObject;

            cardHolder.transform.DestroyChildren(true);

            //create all the card game objects that will be recycled
            for (var c = 0; c < cardBufferSize; c++)
            {
                var cardModel = new CardModel() { playerId = 0, };

                cardService.CreateCard(cardModel, cardHolder.transform);

                createdCards.Add(cardModel);
            }
        }

        void Update()
        {
        }

        internal void RenderInitial()
        {
            DisplayCards(cardDirectory.directory.Skip(offset).Take(pageSize).ToList());
        }

        //Should just be the 8 cards to display
        void DisplayCards(List<CardModel> cards)
        {
            for (int c = 0; c < cardHolder.transform.childCount; c++)
            {
                var childCard = cardHolder.transform.GetChild(c);
            }


            for (int c = 0; c < cards.Count; c++)
            {
                var card = cards[c];
                var cardGameObject = createdCards[c].gameObject;
                cardService.SetupGameObject(card, cardGameObject);

                card.gameObject.transform.SetParent(cardHolder.transform);
                var xPos = 90 + 195 * (c % rowSize);
                var yPos = c >= rowSize ? -415 : -135;

                card.rectTransform.anchorMax = cardAnchor;
                card.rectTransform.anchorMin = cardAnchor;
                card.rectTransform.pivot = cardAnchor;
                card.rectTransform.anchoredPosition3D = new Vector3(xPos, yPos);
            }

        }

    }
}


using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;
using System.Linq;
using System;

namespace ctac
{
    public class CardsMenuView : View
    {
        public Signal clickLeaveSignal = new Signal();

        public Button leaveButton;
        public Button prevButton;
        public Button nextButton;

        ICardService cardService;
        CardDirectory cardDirectory;

        GameObject cardHolder;
        
        const int pageSize = 8;
        const int rowSize = 4;
        const int cardBufferSize = pageSize * 2;
        Vector2 cardAnchor = new Vector2(0, 1);

        List<CardModel> createdCards = new List<CardModel>();

        int offset = 0;
        int prevOffset = -1;

        internal void init(ICardService cs, CardDirectory cd)
        {
            cardService = cs;
            cardDirectory = cd;

            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
            prevButton.onClick.AddListener(onPrevButton);
            nextButton.onClick.AddListener(onNextButton);

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

        internal void UpdateCards()
        {
            if (offset == prevOffset) return; //skip work we don't need to do
            bool isForward = offset > prevOffset;
            prevOffset = offset;
            DisplayCards(cardDirectory.directory.Skip(offset).Take(pageSize).ToList(), isForward);
        }

        //Should just be the 8 cards to display
        int createdCardOffset = 0;
        void DisplayCards(List<CardModel> cards, bool isForward)
        {
            float cardDist = 1200;
            float animTime = 1f;
            Vector3 animDestPosition  = new Vector3(isForward ? -cardDist : cardDist, 0, 0);
            Vector3 animStartPosition = new Vector3(isForward ? cardDist : -cardDist, 0, 0);

            //animate all existing cards out depending on if it's forward or backward
            for (int c = 0; c < cardHolder.transform.childCount; c++)
            {
                var childCard = cardHolder.transform.GetChild(c);
                //iTweenExtensions.MoveToLocal(childCard.gameObject, childCard.transform.position + animDestPosition, animTime, 0f);
                var rectTrans = childCard.GetComponent<RectTransform>();
                rectTrans.anchoredPosition3D = rectTrans.anchoredPosition3D + animDestPosition;
            }


            //now copy all the card props to the surrogate cards and animate them in
            for (int c = 0; c < cards.Count; c++)
            {
                var card = cards[c];
                var cardGameObject = createdCards[c + createdCardOffset].gameObject;
                cardService.SetupGameObject(card, cardGameObject);

                card.gameObject.transform.SetParent(cardHolder.transform);
                var xPos = 90 + 195 * (c % rowSize);
                var yPos = c >= rowSize ? -415 : -135;

                card.rectTransform.anchorMax = cardAnchor;
                card.rectTransform.anchorMin = cardAnchor;
                card.rectTransform.pivot = cardAnchor;
                var destPosition = new Vector3(xPos, yPos);
                card.rectTransform.anchoredPosition3D = animStartPosition + destPosition;
                card.rectTransform.anchoredPosition3D = destPosition;
                //iTweenExtensions.MoveToLocal(card.gameObject, destPosition, animTime, 0f);
            }

            //swip swap the next cards to use by either incrimenting or resetting the created card offset
            createdCardOffset = createdCardOffset == 0 ? pageSize : 0;
        }

        void onPrevButton()
        {
            offset -= pageSize;
            offset = Math.Max(0, offset);
            UpdateCards();
        }

        void onNextButton()
        {
            //don't need to do anything at the end
            if (offset + pageSize >= cardDirectory.directory.Count) return;

            offset += pageSize;
            UpdateCards();
        }

    }
}


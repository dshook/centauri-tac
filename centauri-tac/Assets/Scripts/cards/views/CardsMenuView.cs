using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;
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
        public Slider energySlider;
        public TMP_InputField searchBox;

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
        int? energyFilter = null;
        string stringFilter = null;

        internal void init(ICardService cs, CardDirectory cd)
        {
            cardService = cs;
            cardDirectory = cd;

            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
            prevButton.onClick.AddListener(onPrevButton);
            nextButton.onClick.AddListener(onNextButton);
            energySlider.onValueChanged.AddListener(onEnergySlider);
            searchBox.onValueChanged.AddListener(onSearchChange);

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
            bool isForward = offset > prevOffset;
            prevOffset = offset;

            var cardList = cardDirectory.directory
                .Where(c => !c.uncollectible && !c.isHero)
                .Where(c => !energyFilter.HasValue || c.cost == energyFilter.Value)
                .Where(c => String.IsNullOrEmpty(stringFilter) 
                    || c.name.ToLower().Contains(stringFilter) 
                    || c.description.ToLower().Contains(stringFilter) 
                    || c.tags.Any(t => t.ToLower().Contains(stringFilter))
                )
                .Skip(offset)
                .Take(pageSize)
                .ToList(); 
            DisplayCards(cardList, isForward);
        }

        //Should just be the 8 cards to display
        int createdCardOffset = 0;
        void DisplayCards(List<CardModel> cards, bool isForward)
        {
            float cardDist = 1200;
            //float animTime = 1f;
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
                var surrogateCard = createdCards[c + createdCardOffset];
                var cardGameObject = surrogateCard.gameObject;

                //copy over props from hovered to hover
                card.CopyProperties(surrogateCard);
                //but reset some key things
                surrogateCard.gameObject = cardGameObject;

                //Set up all this reference stuff from the directory card model to the surrogate card that's displaying it
                var cardView = cardGameObject.GetComponent<CardView>();
                if (cardView == null)
                {
                    cardView = cardGameObject.AddComponent<CardView>();
                }
                cardView.card = surrogateCard;
                surrogateCard.rectTransform = cardGameObject.GetComponent<RectTransform>();
                surrogateCard.cardView = cardView;
                cardService.UpdateCardArt(surrogateCard);
                surrogateCard.cardView.UpdateText(0);

                //now position to the grid
                surrogateCard.gameObject.transform.SetParent(cardHolder.transform);
                var xPos = 195 * (c % rowSize);
                var yPos = c >= rowSize ? -419 : -135;

                surrogateCard.rectTransform.anchorMax = cardAnchor;
                surrogateCard.rectTransform.anchorMin = cardAnchor;
                surrogateCard.rectTransform.pivot = cardAnchor;
                var destPosition = new Vector3(xPos, yPos);
                surrogateCard.rectTransform.anchoredPosition3D = animStartPosition + destPosition;
                surrogateCard.rectTransform.anchoredPosition3D = destPosition;
                //iTweenExtensions.MoveToLocal(card.gameObject, destPosition, animTime, 0f);
            }

            //swip swap the next cards to use by either incrimenting or resetting the created card offset
            createdCardOffset = createdCardOffset == 0 ? pageSize : 0;
        }

        void onPrevButton()
        {
            offset -= pageSize;
            offset = Math.Max(0, offset);
            if (offset == prevOffset) return; //skip work we don't need to do
            UpdateCards();
        }

        void onNextButton()
        {
            //don't need to do anything at the end
            if (offset + pageSize >= cardDirectory.directory.Count) return;

            offset += pageSize;
            if (offset == prevOffset) return; //skip work we don't need to do
            UpdateCards();
        }

        void onEnergySlider(float value)
        {
            if (value == -1)
            {
                energyFilter = null;
            }
            else
            {
                energyFilter = (int)value;
            }
            //reset what page we're on when filtering
            offset = 0;
            UpdateCards();
        }

        void onSearchChange(string value)
        {
            stringFilter = value.ToLower();
            offset = 0;
            UpdateCards();
        }

    }
}


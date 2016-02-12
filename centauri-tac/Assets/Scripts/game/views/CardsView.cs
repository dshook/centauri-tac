using ctac.signals;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ctac {
    public class CardsView : View
    {
        //should only be the current players cards
        private List<CardModel> playerCards { get; set; }
        private List<CardModel> opponentCards { get; set; }

        private CardModel selectedCard { get; set; }
        private CardModel hoveredCard { get; set; }

        private Vector2 anchorPosition = new Vector2(0.5f, 0);
        private Vector2 opponentAnchorPosition = new Vector2(0.5f, 1);
        private const float maxCardHeight = 20f;
        private Vector3 dest;

        private Color32 playableCardColor = new Color32(0, 53, 223, 255);
        private Color32 unPlayableCardColor = new Color32(21, 21, 21, 255);

        private float hoverAccumulator = 0f;
        private Vector3 cardCircleCenter = new Vector3(0, -450, 50);
        private Vector3 opponentCardCircleCenter = new Vector3(0, 500, 50);
        private float cardCircleRadius = 420f;
        private float cardAngleSpread = -5f;

        protected override void Start()
        {
        }

        public void init(List<CardModel> playerCards, List<CardModel> opponentCards)
        {
            this.playerCards = playerCards;
            this.opponentCards = opponentCards;
        }

        void Update()
        {
            if(playerCards == null || opponentCards == null) return;

            //position opponents cards
            //might need to DRY it up sometime but rule of three still holds
            cardAngleSpread = -13f + (0.8f * opponentCards.Count);
            for(int c = 0; c < opponentCards.Count; c++) 
            {
                var card = opponentCards[c];
                var rectTransform = card.gameObject.GetComponent<RectTransform>();
                var cardCountOffset = 0 - ((opponentCards.Count - 1) / 2) + c;
                rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, cardCountOffset * cardAngleSpread));
                rectTransform.Rotate(Vector3.up, 180f, Space.Self);

                dest = PointOnCircle(cardCircleRadius, 270f + cardCountOffset * cardAngleSpread, opponentCardCircleCenter);
                dest = dest.SetZ(dest.z + (-1 * c));

                rectTransform.anchorMax = opponentAnchorPosition;
                rectTransform.anchorMin = opponentAnchorPosition;
                rectTransform.pivot = opponentAnchorPosition;
                rectTransform.anchoredPosition3D = iTween.Vector3Update(rectTransform.anchoredPosition3D, dest, 10.0f);
            }

            //and now players cards
            cardAngleSpread = -13f + (0.8f * playerCards.Count);
            for(int c = 0; c < playerCards.Count; c++) 
            {
                var card = playerCards[c];
                var rectTransform = card.gameObject.GetComponent<RectTransform>();
                var cardCountOffset = 0 - ((playerCards.Count - 1) / 2) + c;
                rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, cardCountOffset * cardAngleSpread));

                dest = PointOnCircle(cardCircleRadius, 90f + cardCountOffset * cardAngleSpread, cardCircleCenter);
                dest = dest.SetZ(dest.z + (-1 * c));

                if (selectedCard != null && card == selectedCard)
                {
                    dest = dest.SetY(dest.y + 30f);
                }
                if (hoveredCard != null && card == hoveredCard && hoveredCard != selectedCard)
                {
                    dest = dest.SetY(dest.y + 30f);
                    hoverAccumulator += Time.deltaTime;

                    if (hoverAccumulator > CardView.HOVER_DELAY)
                    {
                        card.cardView.displayWrapper.SetActive(false);
                    }
                }
                rectTransform.anchorMax = anchorPosition;
                rectTransform.anchorMin = anchorPosition;
                rectTransform.pivot = anchorPosition;
                rectTransform.anchoredPosition3D = iTween.Vector3Update(rectTransform.anchoredPosition3D, dest, 10.0f);

                if (card.playable)
                {
                    card.cardView.costText.outlineColor = playableCardColor;
                }
                else
                {
                    card.cardView.costText.outlineColor = unPlayableCardColor;
                }
            }
        }

        private static Vector3 PointOnCircle(float radius, float angleInDegrees, Vector3 origin)
        {
            // Convert from degrees to radians via multiplication by PI/180        
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F)) + origin.x;
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F)) + origin.y;

            return new Vector3(x, y, origin.z);
        }

        internal void onCardSelected(CardModel card)
        {
            selectedCard = card;
        }

        internal void onCardHovered(CardModel card)
        {
            //reset previous hovered card to active
            if (hoveredCard != null)
            {
                hoveredCard.cardView.displayWrapper.SetActive(true);
            }

            //then update
            hoveredCard = card;
            hoverAccumulator = 0f;
        }

        public class CardDestroyedAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public CardDestroyedSignal cardDestroyed { get; set; }
            public CardModel card { get; set; }

            public void Update()
            {
                iTweenExtensions.ScaleTo(card.gameObject, Vector3.zero, 0.5f, 0, EaseType.easeInCubic);
                if (card.gameObject.transform.localScale.x < 0.01f)
                {
                    card.gameObject.transform.localScale = Vector3.zero;
                    Complete = true;
                    cardDestroyed.Dispatch(card);
                }
            }
        }

        public class DrawCardAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return 0.8f; } }

            public CardDrawShownSignal cardDrawn { get; set; }
            public CardModel card { get; set; }

            private float animTime = 1.0f;

            public void Update()
            {
                iTweenExtensions.MoveToLocal(card.gameObject, Vector3.zero, animTime, 0, EaseType.easeOutCubic);
                iTweenExtensions.RotateTo(card.gameObject, Vector3.zero, animTime, 0, EaseType.easeOutCubic);
                if (Vector3.Distance(card.gameObject.transform.localPosition, Vector3.zero) < 0.08f)
                {
                    card.gameObject.transform.localPosition = Vector3.zero;
                    Complete = true;
                    cardDrawn.Dispatch(card);
                }
            }
        }

    }
}

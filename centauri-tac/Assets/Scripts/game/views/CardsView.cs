using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac {
    public class CardsView : View
    {
        //should only be the current players cards
        private List<CardModel> cards { get; set; }

        private CardModel selectedCard { get; set; }
        private CardModel hoveredCard { get; set; }

        private Vector3 baseCardOffset = new Vector3(0, -85f, 50);
        private Vector3 cardPositionOffset = new Vector3(60, 0, -1);
        private Vector2 anchorPosition = new Vector2(0.5f, 0);
        private const float maxCardHeight = 20f;
        private Vector3 dest;

        private CardCanvasHelperView CardCanvasHelper;

        protected override void Start()
        {
            CardCanvasHelper = GameObject.Find(Constants.cardCanvas).GetComponent<CardCanvasHelperView>();
        }

        public void init(List<CardModel> cards)
        {
            this.cards = cards;
        }

        void Update()
        {
            if(cards == null) return;

            for(int c = 0; c < cards.Count; c++) 
            {
                var card = cards[c];
                var rectTransform = card.gameObject.GetComponent<RectTransform>();
                rectTransform.rotation = Quaternion.Euler(Vector3.zero);
                dest = baseCardOffset - ((cards.Count / 2) * cardPositionOffset) + (cardPositionOffset * c);
                if (selectedCard != null && card == selectedCard)
                {
                    dest = dest.SetY(dest.y + 40f);
                }
                if (hoveredCard != null && card == hoveredCard && hoveredCard != selectedCard)
                {
                    dest = dest.SetY(dest.y + 40f);
                }
                rectTransform.anchorMax = anchorPosition;
                rectTransform.anchorMin = anchorPosition;
                rectTransform.pivot = anchorPosition;
                rectTransform.anchoredPosition3D = iTween.Vector3Update(rectTransform.anchoredPosition3D, dest, 10.0f);
            }
        }

        internal void onCardSelected(CardModel card)
        {
            selectedCard = card;
        }

        internal void onCardHovered(CardModel card)
        {
            hoveredCard = card;
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

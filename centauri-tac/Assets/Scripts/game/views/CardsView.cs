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

        private Vector3 baseCardOffset = new Vector3(0, -54f, 0);
        private Vector3 cardPositionOffset = new Vector3(60, 0, -1);
        private const float maxCardHeight = 20f;
        private Vector3 dest;

        private Camera cardCamera;
        private RectTransform CanvasRect;

        protected override void Start()
        {
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == "CardCamera");
            CanvasRect = GameObject.Find("cardCanvas").GetComponent<RectTransform>();
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
                dest = baseCardOffset - ((cards.Count / 2) * cardPositionOffset) + (cardPositionOffset * c);
                if (selectedCard != null)
                {
                    if (card == selectedCard)
                    {
                        var dragPos = MouseToWorld( selectedCard.gameObject.transform.localPosition.z );
                        if (dragPos.y < maxCardHeight)
                        {
                            card.gameObject.transform.localPosition = dragPos;
                            continue;
                        }
                        else
                        {
                            dest = dest.SetY(dest.y + 40f);
                        }
                    }
                }
                rectTransform.anchoredPosition3D = iTween.Vector3Update(rectTransform.anchoredPosition3D, dest, 10.0f);
            }
        }

        private Vector3 MouseToWorld(float z)
        {
            var mouseViewport = cardCamera.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);

            //calculate the position of the UI element
            //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
            return new Vector3(
                ((mouseViewport.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
                ((mouseViewport.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)),
                z
            );
        }

        internal void onCardSelected(CardModel card)
        {
            selectedCard = card;
        }

        public class CardDestroyedAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }

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

    }
}

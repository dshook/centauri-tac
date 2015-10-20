using strange.extensions.mediation.impl;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac {
    public class CardsView : View
    {
        private CardsModel cards { get; set; }

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

        public void init(CardsModel cards)
        {
            this.cards = cards;
        }

        void Update()
        {
            for(int c = 0; c < cards.Cards.Count; c++) 
            {
                var card = cards.Cards[c];
                var rectTransform = card.gameObject.GetComponent<RectTransform>();
                dest = baseCardOffset + (cardPositionOffset * c);
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

    }
}

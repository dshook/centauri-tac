using strange.extensions.mediation.impl;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac {
    public class CardsView : View
    {
        private CardsModel cards { get; set; }

        private CardModel selectedCard { get; set; }

        private Vector3 baseCardOffset;
        private Vector3 cardPositionOffset;

        private Camera cardCamera;

        protected override void Start()
        {
            baseCardOffset = new Vector3(0, -253f, 0);
            cardPositionOffset = new Vector3(60, 0, -1);

            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == "CardCamera");
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
                Vector3 dest = baseCardOffset + (cardPositionOffset * c);
                if (selectedCard != null)
                {
                    if (card == selectedCard)
                    {
                        dest = cardCamera.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
                    }
                }
                iTweenExtensions.MoveToLocal(card.gameObject, dest, 1.0f, 0);
            }
        }

        internal void onCardSelected(CardModel card)
        {
            selectedCard = card;
        }

    }
}

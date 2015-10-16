using ctac.signals;
using strange.extensions.mediation.impl;
using TMPro;
using UnityEngine;

namespace ctac {
    public class CardsView : View
    {
        private CardsModel cards { get; set; }

        private Vector3 baseCardOffset;
        private Vector3 cardPositionOffset;

        protected override void Start()
        {
            baseCardOffset = new Vector3(0, -253f, 0);
            cardPositionOffset = new Vector3(60, 0, -1);
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
                var dest = baseCardOffset + (cardPositionOffset * c);
                iTweenExtensions.MoveToLocal(card.gameObject, dest, 1.0f, 0);
            }
        }

    }
}

using strange.extensions.mediation.impl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac {
    public class DecksView : View
    {
        //should only be the current players cards
        private List<CardModel> deck { get; set; }

        private Vector3 baseCardOffset = new Vector3(24, 0, 0);
        private Vector2 anchorPosition = new Vector2(1, 0.5f);
        private Quaternion baseRotation = Quaternion.Euler(0, 295, 90);

        protected override void Awake()
        {
        }

        public void init(List<CardModel> cards)
        {
            this.deck = cards;

            for(int c = 0; c < deck.Count; c++) 
            {
                var card = cards[c];
                var rectTransform = card.gameObject.GetComponent<RectTransform>();
                rectTransform.localPosition = baseCardOffset + new Vector3(c * 0.3f, 0, 0);
                rectTransform.localRotation = baseRotation;
                rectTransform.localScale = Vector3.one;
                rectTransform.anchorMax = anchorPosition;
                rectTransform.anchorMin = anchorPosition;
                rectTransform.pivot = anchorPosition;

                //why do I have to set this again?
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 0);
            }
        }

        void Update()
        {
            //if(deck == null) return;

        }

    }
}

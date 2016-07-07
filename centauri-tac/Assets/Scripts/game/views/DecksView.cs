using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;

namespace ctac {
    public class DecksView : View
    {
        private Vector3 baseCardOffset = new Vector3(24, 0, 0);
        private Vector2 anchorPosition = new Vector2(1, 0.5f);
        private Quaternion baseRotation = Quaternion.Euler(0, 295, 90);
        private GameObject DeckGO;
        private GameObject OpponentDeckGO;

        protected override void Awake()
        {
            DeckGO = GameObject.Find("Deck");
            OpponentDeckGO = GameObject.Find("OpponentDeck");
        }

        public void UpdatePositions(List<CardModel> playerCards, List<CardModel> opponentCards)
        {
            for(int c = 0; c < playerCards.Count; c++) 
            {
                SetCardPos(playerCards[c], c, false);
            }

            for(int c = 0; c < opponentCards.Count; c++) 
            {
                SetCardPos(opponentCards[c], c, true);
            }
        }

        private void SetCardPos(CardModel card, int c, bool isOpponent)
        {
            if (isOpponent)
            {
                card.gameObject.transform.SetParent(OpponentDeckGO.transform, false);
            }
            else
            {
                card.gameObject.transform.SetParent(DeckGO.transform, false);
            }

            var rectTransform = card.rectTransform;
            rectTransform.anchorMax = anchorPosition;
            rectTransform.anchorMin = anchorPosition;
            rectTransform.anchoredPosition3D = baseCardOffset + new Vector3(c * 0.3f, 0, 0);
            rectTransform.localRotation = baseRotation;
            rectTransform.localScale = Vector3.one;
            rectTransform.pivot = anchorPosition;
        }

        void Update()
        {

        }

    }
}

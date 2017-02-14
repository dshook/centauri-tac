using strange.extensions.mediation.impl;
using TMPro;
using UnityEngine;
using System.Linq;

namespace ctac
{
    public class DeckHoverView : View
    {
        [Inject]
        public RaycastModel raycastModel { get; set; }

        [Inject] public DecksModel decks { get; set; }
        [Inject] public GameTurnModel gameTurn { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        Vector3 offset = new Vector3(-85f, 0, 0);
        private GameObject DeckGO;
        private GameObject OpponentDeckGO;

        TextMeshProUGUI text;
        GameObject displayWrapper;
        RectTransform rectTransform;


        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            displayWrapper = transform.Find("DisplayWrapper").gameObject;
            text = displayWrapper.GetComponentInChildren<TextMeshProUGUI>();
            DeckGO = GameObject.Find("Deck");
            OpponentDeckGO = GameObject.Find("OpponentDeck");
        }


        void Update()
        {
            var hoverHit = raycastModel.cardCanvasHit;

            var hoverOpponent = hoverHit.HasValue
                && hoverHit.Value.collider != null
                && hoverHit.Value.collider.gameObject.transform.parent.name == "OpponentDeck";
            var hoverDeck = hoverHit.HasValue
                && hoverHit.Value.collider != null
                && hoverHit.Value.collider.gameObject.transform.parent.name == "Deck";

            //check to see if a card in a deck has been hovered
            //also check to see if the hit object has been destroyed in the meantime
            if (hoverOpponent)
            {
                displayWrapper.SetActive(true);
                rectTransform.SetParent(OpponentDeckGO.transform, false);
                rectTransform.anchoredPosition3D = offset;
                text.text = decks.Cards.Count(c => c.playerId != players.Me.id).ToString();
            }

            if (hoverDeck)
            {
                displayWrapper.SetActive(true);
                rectTransform.SetParent(DeckGO.transform, false);
                rectTransform.anchoredPosition3D = offset;
                text.text = decks.Cards.Count(c => c.playerId == players.Me.id).ToString();
            }

            if (!hoverOpponent && !hoverDeck)
            {
                displayWrapper.SetActive(false);
            }
        }
    }
}


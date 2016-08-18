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


        Vector3 opponentDeckPosition = new Vector3(-14.7f, 69.3f, 0);
        Vector3 friendlyDeckPosition = new Vector3(-14.7f, -86.5f, 0);

        TextMeshProUGUI text;
        GameObject displayWrapper;
        RectTransform rectTransform;

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            displayWrapper = transform.Find("DisplayWrapper").gameObject;
            text = displayWrapper.GetComponentInChildren<TextMeshProUGUI>();
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
                rectTransform.anchoredPosition3D = opponentDeckPosition;
                text.text = decks.Cards.Count(c => c.playerId == players.OpponentId(gameTurn.currentPlayerId)).ToString();
            }

            if (hoverDeck)
            {
                displayWrapper.SetActive(true);
                rectTransform.anchoredPosition3D = friendlyDeckPosition;
                text.text = decks.Cards.Count(c => c.playerId != players.OpponentId(gameTurn.currentPlayerId)).ToString();
            }

            if (!hoverOpponent && !hoverDeck)
            {
                displayWrapper.SetActive(false);
            }
        }
    }
}


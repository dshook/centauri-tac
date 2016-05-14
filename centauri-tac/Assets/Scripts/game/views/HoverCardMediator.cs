using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class HoverCardMediator : Mediator
    {
        [Inject]
        public HoverCardView view { get; set; }
        
        [Inject] public CardHoveredSignal cardHovered { get; set; }
        [Inject] public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public PieceHoverSignal pieceHovered { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        public override void OnRegister()
        {
            view.init();
            cardHovered.AddListener(onCardHovered);
            cardSelected.AddListener(onCardSelected);
            pieceHovered.AddListener(onPieceHovered);
        }

        public override void onRemove()
        {
            cardHovered.RemoveListener(onCardHovered);
            cardSelected.RemoveListener(onCardSelected);
            pieceHovered.RemoveListener(onPieceHovered);
        }

        private void onCardHovered(CardModel card)
        {
            if (card != null)
            {
                var hoveredCardRect = card.gameObject.GetComponent<RectTransform>();
                var position = hoveredCardRect.anchoredPosition3D;
                view.showCardFromHand(card, position);
            }
            else
            {
                view.hideCard();
            }
        }

        private void onCardSelected(CardSelectedModel card)
        {
            view.setActive(card == null);
        }

        private void onPieceHovered(PieceModel piece)
        {
            if (piece != null)
            {
                var card = cardDirectory.Card(piece.cardTemplateId);
                view.showCardWorld(card, piece.gameObject.transform.position);
            }
            else
            {
                view.hideCard();
            }
        }

    }
}


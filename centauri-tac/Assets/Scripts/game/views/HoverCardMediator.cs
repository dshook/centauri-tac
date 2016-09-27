using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class HoverCardMediator : Mediator
    {
        [Inject]
        public HoverCardView view { get; set; }
        
        [Inject] public CardHoveredSignal cardHovered { get; set; }
        [Inject] public CardSelectedSignal cardSelected { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject]
        public PieceHoverSignal pieceHovered { get; set; }

        //[Inject] public IDebugService debug { get; set; }

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

        private bool hoveringCard = false;

        private void onCardHovered(CardModel card)
        {
            if (card != null)
            {
                var hoveredCardRect = card.rectTransform;
                var position = hoveredCardRect.anchoredPosition3D;
                view.showCardFromHand(card, position, possibleActions.GetSpellDamage(card.playerId));
                hoveringCard = true;
                //debug.Log("Hovering from card");
            }
            else
            {
                view.hideCard();
                hoveringCard = false;
                //debug.Log("Hiding from card");
            }
        }

        private void onCardSelected(CardSelectedModel card)
        {
            view.setActive(card == null);
            view.hideCard();
            //debug.Log("Setting hover view " + (card == null ? "active" : "inactive"));
        }

        private void onPieceHovered(PieceModel piece)
        {
            //hovering cards takes priority so don't hide/show if we're hovering from a card
            if(hoveringCard) return;

            if (piece != null)
            {
                view.showPieceCardWorld(piece, piece.gameObject.transform.position, possibleActions.GetSpellDamage(piece.playerId));
                //debug.Log("Hovering from piece");
            }
            else 
            {
                view.hideCard();
                //debug.Log("Hiding from piece");
            }
        }

    }
}


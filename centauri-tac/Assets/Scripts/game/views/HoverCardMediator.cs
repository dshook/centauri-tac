using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class HoverCardMediator : Mediator
    {
        [Inject] public HoverCardView view { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        //[Inject] public IDebugService debug { get; set; }

        public override void OnRegister()
        {
            view.init();
        }

        private bool hoveringCard = false;

        [ListensTo(typeof(CardHoveredSignal))]
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

        [ListensTo(typeof(CardSelectedSignal))]
        private void onCardSelected(CardSelectedModel card)
        {
            view.setActive(card == null);
            view.hideCard();
            //debug.Log("Setting hover view " + (card == null ? "active" : "inactive"));
        }

        [ListensTo(typeof(PieceHoverSignal))]
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

        [ListensTo(typeof(MiniCardHoveredSignal))]
        private void onMiniCardHovered(CardModel card)
        {
            if (card != null)
            {
                var hoveredCardRect = card.rectTransform;
                var position = hoveredCardRect.anchoredPosition3D;
                view.showMiniCardFromDeck(card, position, 0);
                hoveringCard = true;
            }
            else
            {
                view.hideCard();
                hoveringCard = false;
            }
        }
    }
}


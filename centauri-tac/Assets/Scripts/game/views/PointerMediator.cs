using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PointerMediator : Mediator
    {
        [Inject]
        public PointerView view { get; set; }

        [Inject]
        public CardSelectedSignal cardStartDrag { get; set; }

        [Inject]
        public ActivateCardSignal cardActivated { get; set; }

        public override void OnRegister()
        {
            view.init();

            cardStartDrag.AddListener(onCardDragStart);
            cardActivated.AddListener(onCardDragEnd);
        }

        public override void onRemove()
        {
            cardStartDrag.RemoveListener(onCardDragStart);
            cardActivated.RemoveListener(onCardDragEnd);
        }

        private void onCardDragStart(CardModel card)
        {
            if (card != null && card.gameObject != null)
            {
                view.rectTransform(card.gameObject);
            }
        }

        private void onCardDragEnd(CardModel card, Tile t)
        {
            view.disable();
        }

    }
}


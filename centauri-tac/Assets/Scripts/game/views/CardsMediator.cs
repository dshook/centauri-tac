using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class CardsMediator : Mediator
    {
        [Inject]
        public CardsView view { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        public override void OnRegister()
        {
            view.init(cards);
            cardSelected.AddListener(onCardSelected);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
        }

        private void onCardSelected(CardModel card)
        {
            view.onCardSelected(card);
        }

    }
}


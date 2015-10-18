using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class CameraMovementMediator : Mediator
    {
        [Inject]
        public CameraMovementView view { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        public override void OnRegister()
        {
            cardSelected.AddListener(onCardSelected);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
        }

        private void onCardSelected(CardModel card)
        {
            view.onCardSelected(card != null);
        }
    }
}


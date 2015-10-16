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

        public override void OnRegister()
        {
            view.init(cards);
        }

    }
}


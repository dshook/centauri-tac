using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{

    public class CursorMediator : Mediator
    {
        [Inject] public CursorView view { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        public override void OnRegister()
        {
            view.init(loader);
        }

        [ListensTo(typeof(CursorSignal))]
        public void onMessage(CursorStyles style)
        {
            view.setStyle(style);
        }

    }
}


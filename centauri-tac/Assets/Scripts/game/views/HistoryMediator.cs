using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class HistoryMediator : Mediator
    {
        [Inject]
        public AbilityView view { get; set; }

        [Inject] public ServerQueueProcessStart qps { get; set; }
        [Inject] public ServerQueueProcessEnd qpc { get; set; }

        public override void OnRegister()
        {
            view.init();
        }

        public override void onRemove()
        {
        }


    }
}


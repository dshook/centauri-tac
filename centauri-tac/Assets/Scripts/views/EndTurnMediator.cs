using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class EndTurnMediator : Mediator
    {
        [Inject]
        public EndTurnView view { get; set; }

        [Inject]
        public EndTurnSignal endTurnSignal { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onTurnClicked);
            turnEnded.AddListener(onTurnEnded);
            view.init();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onTurnClicked);
            turnEnded.RemoveListener(onTurnEnded);
        }

        private void onTurnClicked()
        {
            endTurnSignal.Dispatch();
        }

        private void onTurnEnded()
        {
            view.onTurnEnded();
        }

    }
}


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
        public ISocketService socket { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

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
            socket.Request(gameTurn.currentTurnClientId, "game", "endTurn");
            endTurnSignal.Dispatch();
        }

        private void onTurnEnded()
        {
            view.onTurnEnded();
        }

    }
}


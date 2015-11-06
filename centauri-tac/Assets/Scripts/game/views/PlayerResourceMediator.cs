using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PlayerResourceMediator : Mediator
    {
        [Inject]
        public PlayerResourceView view { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        [Inject]
        public PlayerResourcesModel playerResources { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        public override void OnRegister()
        {
            turnEnded.AddListener(onTurnEnded);
            view.init();
        }

        public override void onRemove()
        {
            turnEnded.RemoveListener(onTurnEnded);
        }

        private void onTurnClicked()
        {
        }

        private void onTurnEnded()
        {
            var currentResource = playerResources.resources[gameTurn.currentPlayerId];
            //TODO: un hardcode max
            view.updateText(currentResource, 10);
        }

    }
}


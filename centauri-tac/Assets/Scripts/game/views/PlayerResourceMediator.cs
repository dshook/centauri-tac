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
        public PlayerResourceSetSignal resourceSet { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        public override void OnRegister()
        {
            resourceSet.AddListener(onResourceSet);
            turnEnded.AddListener(onResourcesUpdated);
            view.init();
        }

        public override void onRemove()
        {
            resourceSet.RemoveListener(onResourceSet);
            turnEnded.RemoveListener(onResourcesUpdated);
        }

        private void onResourceSet(SetPlayerResourceModel m)
        {
            onResourcesUpdated();
        }

        private void onResourcesUpdated()
        {
            var currentResource = playerResources.resources[gameTurn.currentPlayerId];
            //TODO: un hardcode max
            view.updateText(currentResource, 20);
        }

    }
}


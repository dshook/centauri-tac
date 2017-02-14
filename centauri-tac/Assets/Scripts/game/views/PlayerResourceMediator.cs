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
        public PlayerResourcesModel playerResources { get; set; }

        [Inject]
        public PlayerResourceSetSignal resourceSet { get; set; }

        public override void OnRegister()
        {
            resourceSet.AddListener(onResourceSet);
            view.init();
        }

        public override void onRemove()
        {
            resourceSet.RemoveListener(onResourceSet);
        }

        private void onResourceSet(SetPlayerResourceModel m)
        {
            playerResources.resources[m.playerId] = m.newAmount;
            playerResources.maxResources[m.playerId] = m.newMax;
        }

    }
}


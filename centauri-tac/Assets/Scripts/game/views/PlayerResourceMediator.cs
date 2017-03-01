using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PlayerResourceMediator : Mediator
    {
        [Inject] public PlayerResourceView view { get; set; }

        [Inject] public PlayerResourceSetSignal resourceSet { get; set; }
        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public CurrentGameModel currentGame { get; set; }
        [Inject] public IDebugService debug { get; set; }


        public override void OnRegister()
        {
            resourceSet.AddListener(onResourceSet);
            turnEnded.AddListener(onTurnEnd);

            view.init();
        }

        public override void onRemove()
        {
            resourceSet.RemoveListener(onResourceSet);
            turnEnded.RemoveListener(onTurnEnd);
        }

        public void onTurnEnd(GameTurnModel passTurn)
        {
            updateView();

            view.updatePreview(playerResources.maxResources[players.Me.id], currentGame.game.turnLengthMs);
        }

        private void onResourceSet(SetPlayerResourceModel m)
        {
            playerResources.resources[m.playerId] = m.newAmount;
            playerResources.maxResources[m.playerId] = m.newMax;
            updateView();
            if (m.newMax == 0)
            {
                view.updatePreview(m.newMax, currentGame.game.turnLengthMs);
            }
        }

        private void updateView()
        {
            view.updateText(playerResources.resources[players.Me.id], playerResources.maxResources[players.Me.id]);
        }

    }
}


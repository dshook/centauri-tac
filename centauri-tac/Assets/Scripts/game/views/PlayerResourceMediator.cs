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

        [Inject] public GamePausedSignal pauseSignal { get; set; }
        [Inject] public GameResumedSignal resumeSignal { get; set; }

        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public CurrentGameModel currentGame { get; set; }
        [Inject] public IDebugService debug { get; set; }


        public override void OnRegister()
        {
            resourceSet.AddListener(onResourceSet);
            turnEnded.AddListener(onTurnEnd);
            pauseSignal.AddListener(onPause);
            resumeSignal.AddListener(onResume);

            view.init();
        }

        public override void onRemove()
        {
            resourceSet.RemoveListener(onResourceSet);
            turnEnded.RemoveListener(onTurnEnd);
            pauseSignal.RemoveListener(onPause);
            resumeSignal.RemoveListener(onResume);
        }

        public void onTurnEnd(GameTurnModel passTurn)
        {
            updateView();

            view.updatePreview(playerResources.maxResources[playerId], currentGame.game.turnLengthMs);
        }

        private void onResourceSet(SetPlayerResourceModel m)
        {
            //if not for me, leave it be
            if(m.playerId != playerId) return;

            updateView();
            if (m.newMax == 0)
            {
                view.updatePreview(m.newMax, currentGame.game.turnLengthMs);
            }
        }

        private void onPause()
        {
            view.setOn(false);
        }
        
        private void onResume()
        {
            view.setOn(true);
        }

        private void updateView()
        {
            view.updateText(playerResources.resources[playerId], playerResources.maxResources[playerId]);
        }

        private int playerId
        {
            get
            {
                return view.isOpponent ? players.Opponent(players.Me.id).id : players.Me.id;
            }
        }

    }
}


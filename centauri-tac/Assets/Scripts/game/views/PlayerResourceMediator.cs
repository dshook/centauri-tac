using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PlayerResourceMediator : Mediator
    {
        [Inject] public PlayerResourceView view { get; set; }

        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public CurrentGameModel currentGame { get; set; }
        [Inject] public GameTurnModel turnModel { get; set; }

        [Inject] public ISoundService sounds { get; set; }
        [Inject] public IDebugService debug { get; set; }


        public override void OnRegister()
        {
            view.init(sounds);
        }

        [ListensTo(typeof(ActionKickoffSignal))]
        public void onKickoff(KickoffModel km, SocketKey key)
        {
            SetTimers();
        }

        [ListensTo(typeof(TurnEndedSignal))]
        public void onTurnEnd(GameTurnModel passTurn)
        {
            updateView();
            SetTimers();

            if (!passTurn.isClientSwitch)
            {
                view.updatePreview(playerResources.maxResources[playerId]);
            }
        }

        [ListensTo(typeof(PlayerResourceSetSignal))]
        public void onResourceSet(SetPlayerResourceModel m)
        {
            //if not for me, leave it be
            if(m.playerId != playerId) return;

            updateView();
            if (m.newMax == 0)
            {
                view.updatePreview(m.newMax);
            }
        }

        private void SetTimers()
        {
            //Have to subtract one from the turn here to sync up due to the ordering of when the intervals are set in the server
            view.setTimers(turnLength(currentGame.game, turnModel.currentTurn - 1), currentGame.game.turnEndBufferLengthMs);
        }

        [ListensTo(typeof(GamePausedSignal))]
        public void onPause()
        {
            view.setOn(false);
        }

        [ListensTo(typeof(GameResumedSignal))]
        public void onResume()
        {
            view.setOn(true);
        }

        [ListensTo(typeof(GameFinishedSignal))]
        public void onGameFinished(GameFinishedModel gf)
        {
            view.setOn(false);
        }

        private void updateView()
        {
            view.setEnergy(playerResources.resources[playerId], playerResources.maxResources[playerId], !view.isOpponent);
        }

        private int playerId
        {
            get
            {
                return view.isOpponent ? players.Opponent(players.Me.id).id : players.Me.id;
            }
        }

        private int turnLength(GameMetaModel game, int currentTurn)
        {
            return game.turnLengthMs + (currentTurn * game.turnIncrementLengthMs);
        }


    }
}


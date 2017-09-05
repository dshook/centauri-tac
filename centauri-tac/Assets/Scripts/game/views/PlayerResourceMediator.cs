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
        [Inject] public GameFinishedSignal gameFinished { get; set; }
        [Inject] public ActionKickoffSignal kickoff { get; set; }

        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public CurrentGameModel currentGame { get; set; }
        [Inject] public GameTurnModel turnModel { get; set; }

        [Inject] public ISoundService sounds { get; set; }
        [Inject] public IDebugService debug { get; set; }


        public override void OnRegister()
        {
            resourceSet.AddListener(onResourceSet);
            turnEnded.AddListener(onTurnEnd);
            pauseSignal.AddListener(onPause);
            resumeSignal.AddListener(onResume);
            gameFinished.AddListener(onGameFinished);
            kickoff.AddListener(onKickoff);

            view.init(sounds);
        }

        public override void onRemove()
        {
            resourceSet.RemoveListener(onResourceSet);
            turnEnded.RemoveListener(onTurnEnd);
            pauseSignal.RemoveListener(onPause);
            resumeSignal.RemoveListener(onResume);
            gameFinished.RemoveListener(onGameFinished);
            kickoff.RemoveListener(onKickoff);
        }

        public void onKickoff(KickoffModel km, SocketKey key)
        {
            SetTimers();
        }

        public void onTurnEnd(GameTurnModel passTurn)
        {
            updateView();
            SetTimers();

            if (!passTurn.isClientSwitch)
            {
                view.updatePreview(playerResources.maxResources[playerId]);
            }
        }

        private void onResourceSet(SetPlayerResourceModel m)
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

        private void onPause()
        {
            view.setOn(false);
        }
        
        private void onResume()
        {
            view.setOn(true);
        }

        private void onGameFinished(GameFinishedModel gf)
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


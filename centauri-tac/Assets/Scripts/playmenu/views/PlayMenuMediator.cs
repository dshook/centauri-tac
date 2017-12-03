using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ctac
{
    public class PlayMenuMediator : Mediator
    {
        [Inject] public PlayMenuView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public MatchmakerQueueSignal mmQueue { get; set; }
        [Inject] public MatchmakerDequeueSignal mmDequeue { get; set; }

        [Inject] public SwitchLobbyViewSignal moveLobbyView { get; set; }
        [Inject] public LobbyModel lobbyModel { get; set; }

        DeckModel selectedDeck = null;

        public override void OnRegister()
        {
            view.init();

            view.playButton.onClick.AddListener(onPlayClicked);
            view.leaveButton.onClick.AddListener(onLeaveClicked);
        }

        public override void OnRemove()
        {
        }

        private void onPlayClicked()
        {
            if (!view.queueing && lobbyModel.lobbyKey != null)
            {
                mmQueue.Dispatch(new QueueModel() { deckId = selectedDeck != null ? (int?)selectedDeck.id : null }, lobbyModel.lobbyKey);
            }
            if (view.queueing && lobbyModel.lobbyKey != null)
            {
                mmDequeue.Dispatch(lobbyModel.lobbyKey);
            }
        }

        private void onLeaveClicked()
        {
            moveLobbyView.Dispatch(LobbyScreens.main);
        }

        [ListensTo(typeof(MatchmakerStatusSignal))]
        private void onMatchmakerStatus(MatchmakerStatusModel model, SocketKey key)
        {
            lobbyModel.lobbyKey = key;
            if(model.beingMatched){
                view.SetButtonsActive(false);
                view.SetLoadingProgress(0);
            }else{
                view.SetQueueing(model.inQueue);
            }
        }

        [ListensTo(typeof(CurrentGameSignal))]
        private void onCurrentGame(GameMetaModel game, SocketKey key)
        {
            view.SetButtonsActive(false);
            view.SetLoadingProgress(0.05f);
        }

        [ListensTo(typeof(GameLoggedInSignal))]
        private void onGameLoggedIn(LoginStatusModel gameLogin, SocketKey key)
        {
            if (gameLogin.status)
            {
                StartCoroutine("LoadLevel", "1");
            }
            else
            {
                //TODO more graceful handling?
                debug.LogError("Could not log into game " + gameLogin.message);
            }
        }

        [ListensTo(typeof(SelectDeckSignal))]
        public void onSelectDeck(DeckModel deck)
        {
            selectedDeck = deck;
            if(deck != null){
                view.playButton.interactable = true;
            }
        }

        public IEnumerator LoadLevel(string level)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);

            while (async.progress < 0.9f)
            {
                var scaledPerc = 0.5f * async.progress / 0.9f;
                view.SetLoadingProgress(scaledPerc);
            }

            async.allowSceneActivation = true;
            float perc = 0.5f;
            while (!async.isDone)
            {
                yield return null;
                perc = Mathf.Lerp(perc, 1f, 0.05f);
                view.SetLoadingProgress(perc);
            }

            view.SetLoadingProgress(1.0f);

        }


    }
}


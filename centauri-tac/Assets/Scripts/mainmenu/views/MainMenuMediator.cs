using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ctac
{
    public class MainMenuMediator : Mediator
    {
        [Inject] public MainMenuView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public NeedLoginSignal needLogin { get; set; }
        [Inject] public AuthLoggedInSignal authLoggedIn { get; set; }
        [Inject] public PlayerFetchedFinishedSignal playerFetched { get; set; }

        [Inject] public AuthMatchmakerSignal authMatchmaker { get; set; }
        [Inject] public MatchmakerLoggedInSignal mmLoggedIn { get; set; }
        [Inject] public MatchmakerQueueSignal mmQueue { get; set; }
        [Inject] public MatchmakerDequeueSignal mmDequeue { get; set; }
        [Inject] public MatchmakerStatusSignal mmStatus { get; set; }

        [Inject] public GameLoggedInSignal currentGame { get; set; }

        PlayerModel loggedInPlayer = null;
        SocketKey loggedInKey = null;
        SocketKey mmKey = null;

        public override void OnRegister()
        {
            view.clickPlaySignal.AddListener(onPlayClicked);
            view.clickCardsSignal.AddListener(onCardsClicked);
            view.clickOptionsSignal.AddListener(onOptionsClicked);
            view.clickAboutSignal.AddListener(onAboutClicked);
            view.clickLeaveSignal.AddListener(onLeaveClicked);

            authLoggedIn.AddListener(onLogin);
            needLogin.AddListener(onNeedLogin);
            playerFetched.AddListener(onPlayerFetched);
            mmStatus.AddListener(onMatchmakerStatus);
            mmLoggedIn.AddListener(onMatchmakerLoggedIn);

            currentGame.AddListener(onCurrentGame);

            view.init();
        }

        public override void onRemove()
        {
            view.clickPlaySignal.RemoveListener(onPlayClicked);
            view.clickCardsSignal.RemoveListener(onCardsClicked);
            view.clickOptionsSignal.RemoveListener(onOptionsClicked);
            view.clickAboutSignal.RemoveListener(onAboutClicked);
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);

            authLoggedIn.RemoveListener(onLogin);
            needLogin.RemoveListener(onNeedLogin);
            playerFetched.RemoveListener(onPlayerFetched);
            mmStatus.RemoveListener(onMatchmakerStatus);
            mmLoggedIn.RemoveListener(onMatchmakerLoggedIn);

            currentGame.RemoveListener(onCurrentGame);
        }

        public void Update()
        {
        }

        private void onPlayClicked()
        {
            if (!view.queueing)
            {
                if (mmKey == null && loggedInPlayer != null && loggedInKey != null) {
                    authMatchmaker.Dispatch(loggedInPlayer, loggedInKey);
                }
                if (mmKey != null)
                {
                    mmQueue.Dispatch(mmKey);
                }
            }
            if (view.queueing && mmKey != null)
            {
                mmDequeue.Dispatch(mmKey);
            }
        }

        private void onCardsClicked()
        {
        }

        private void onOptionsClicked()
        {
        }

        private void onAboutClicked()
        {
        }

        private void onLeaveClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        private void onNeedLogin()
        {
            view.SetButtonsActive(false);
        }

        private void onLogin(LoginStatusModel model, SocketKey key)
        {
            if (model.status)
            {
                view.SetButtonsActive(model.status);
            }
        }

        private void onPlayerFetched(PlayerModel player, SocketKey key)
        {
            loggedInPlayer = player;
            loggedInKey = key;
            view.SetUsername("Welcome " + player.email.Substring(0, player.email.IndexOf('@')));
            view.enableButtons();
        }

        private void onMatchmakerLoggedIn(LoginStatusModel loginStatus, SocketKey key)
        {
            mmKey = key;
            if (loginStatus.status == false)
            {
                debug.LogError("Could not log into matchmaker service: " + loginStatus.message);
                return;
            }
            mmQueue.Dispatch(mmKey);
        }

        private void onMatchmakerStatus(MatchmakerStatusModel model, SocketKey key)
        {
            mmKey = key;
            view.SetQueueing(model.inQueue);
        }

        private void onCurrentGame(LoginStatusModel gameLogin, SocketKey key)
        {
            if (gameLogin.status)
            {
                StartCoroutine("LoadLevel");
            }
            else
            {
                //TODO more graceful handling?
                debug.LogError("Could not log into game " + gameLogin.message);
            }
        }

        public IEnumerator LoadLevel()
        {
            view.SetButtonsActive(false);
            AsyncOperation async = SceneManager.LoadSceneAsync("1", LoadSceneMode.Single);

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


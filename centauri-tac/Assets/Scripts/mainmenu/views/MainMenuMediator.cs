using ctac.signals;
using strange.extensions.mediation.impl;
using UnityEngine;

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

            needLogin.AddListener(onNeedLogin);
            playerFetched.AddListener(onPlayerFetched);
            mmStatus.AddListener(onMatchmakerStatus);
            mmLoggedIn.AddListener(onMatchmakerLoggedIn);

            view.init();
        }

        public override void onRemove()
        {
            view.clickPlaySignal.RemoveListener(onPlayClicked);
            view.clickCardsSignal.RemoveListener(onCardsClicked);
            view.clickOptionsSignal.RemoveListener(onOptionsClicked);
            view.clickAboutSignal.RemoveListener(onAboutClicked);
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);

            needLogin.RemoveListener(onNeedLogin);
            playerFetched.RemoveListener(onPlayerFetched);
            mmStatus.RemoveListener(onMatchmakerStatus);
            mmLoggedIn.RemoveListener(onMatchmakerLoggedIn);
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
            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            Application.Quit();
        }

        private void onNeedLogin()
        {
            view.SetButtonsActive(false);
        }

        private void onLogin(LoginStatusModel model, SocketKey key)
        {
            view.SetButtonsActive(model.status);
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
    }
}


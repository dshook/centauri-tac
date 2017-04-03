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

        PlayerModel loggedInPlayer = null;
        SocketKey loggedInKey = null;

        public override void OnRegister()
        {
            view.clickPlaySignal.AddListener(onPlayClicked);
            view.clickCardsSignal.AddListener(onCardsClicked);
            view.clickOptionsSignal.AddListener(onOptionsClicked);
            view.clickLeaveSignal.AddListener(onLeaveClicked);

            needLogin.AddListener(onNeedLogin);
            playerFetched.AddListener(onPlayerFetched);

            view.init();
        }

        public override void onRemove()
        {
            view.clickPlaySignal.RemoveListener(onPlayClicked);
            view.clickCardsSignal.RemoveListener(onCardsClicked);
            view.clickOptionsSignal.RemoveListener(onOptionsClicked);
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);

            needLogin.RemoveListener(onNeedLogin);
            playerFetched.RemoveListener(onPlayerFetched);
        }

        public void Update()
        {
        }

        private void onPlayClicked()
        {
            if (loggedInPlayer != null && loggedInKey != null)
            {
                authMatchmaker.Dispatch(loggedInPlayer, loggedInKey);
            }
        }

        private void onCardsClicked()
        {
        }

        private void onOptionsClicked()
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
    }
}


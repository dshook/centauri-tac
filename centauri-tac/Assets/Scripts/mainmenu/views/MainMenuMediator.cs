using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ctac
{
    public class MainMenuMediator : Mediator
    {
        [Inject] public MainMenuView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public AuthLogoutSignal authLogout { get; set; }

        [Inject] public AuthLobbySignal authLobby { get; set; }
        [Inject] public ServerAuthSignal serverAuthSignal { get; set; }

        [Inject] public CancelDeckSignal cancelDeck { get; set; }

        [Inject] public SwitchLobbyViewSignal moveLobbyView { get; set; }
        [Inject] public LobbyModel lobbyModel { get; set; }

        PlayerModel loggedInPlayer = null;
        SocketKey loggedInKey = null;

        public override void OnRegister()
        {
            lobbyModel.cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == Constants.cardCamera);
            lobbyModel.cardCamera.transform.position = LobbyModel.lobbyPositions[LobbyScreens.main];

            view.clickPlaySignal.AddListener(onPlayClicked);
            view.clickCardsSignal.AddListener(onCardsClicked);
            view.clickOptionsSignal.AddListener(onOptionsClicked);
            view.clickAboutSignal.AddListener(onAboutClicked);
            view.clickLeaveSignal.AddListener(onLeaveClicked);
            view.clickLogoutSignal.AddListener(onLogoutClicked);

            view.init();
        }

        public override void OnRemove()
        {
            view.clickPlaySignal.RemoveListener(onPlayClicked);
            view.clickCardsSignal.RemoveListener(onCardsClicked);
            view.clickOptionsSignal.RemoveListener(onOptionsClicked);
            view.clickAboutSignal.RemoveListener(onAboutClicked);
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);
            view.clickLogoutSignal.RemoveListener(onLogoutClicked);
        }

        public void Update()
        {
        }

        private void onPlayClicked()
        {
            moveLobbyView.Dispatch(LobbyScreens.play);
        }

        private void onCardsClicked()
        {
            moveLobbyView.Dispatch(LobbyScreens.cards);
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

        private void onLogoutClicked()
        {
            authLogout.Dispatch();
        }

        [ListensTo(typeof(NeedLoginSignal))]
        public void onNeedLogin()
        {
            view.SetButtonsActive(false);
            view.SetUsername("");
        }

        [ListensTo(typeof(AuthLoggedInSignal))]
        public void onLogin(LoginStatusModel model, SocketKey key)
        {
            if (model.status)
            {
            }
        }

        [ListensTo(typeof(PlayerFetchedFinishedSignal))]
        private void onPlayerFetched(PlayerModel player, SocketKey key)
        {
            loggedInPlayer = player;
            loggedInKey = key;
            view.SetUsername("Welcome " + player.email.Substring(0, player.email.IndexOf('@')));
            view.setMessage("");
            authLobby.Dispatch(loggedInPlayer, loggedInKey);
        }

        [ListensTo(typeof(LobbyLoggedInSignal))]
        private void onLobbyLoggedIn(LoginStatusModel loginStatus, SocketKey key)
        {
            lobbyModel.lobbyKey = key;
            if (loginStatus.status == false)
            {
                debug.LogError("Could not log into lobby service: " + loginStatus.message);
                view.setMessage("Lobby server unavailable now, please try again later");
                return;
            }
            view.SetButtonsActive(loginStatus.status);
            view.enableButtons();
        }

        [ListensTo(typeof(SocketHangupSignal))]
        public void onSocketDisconnect(SocketKey key)
        {
            if (key.componentName == "auth")
            {
                view.setMessage("Cannot connect to server,\nplease check your connection and retry");
            }
            else
            {
                view.setMessage("Lost connection to server, please try again later");
            }
            view.disableButtons();
            moveLobbyView.Dispatch(LobbyScreens.main);
            StartCoroutine(retryConnection());
        }

        private IEnumerator retryConnection()
        {
            yield return new WaitForSeconds(3f);
            debug.Log("Retrying connection for main menu");
            serverAuthSignal.Dispatch();
        }

        [ListensTo(typeof(SwitchLobbyViewSignal))]
        public void onChangeLobbyScreen(LobbyScreens screen)
        {
            var position = LobbyModel.lobbyPositions[screen];

            lobbyModel.cardCamera.gameObject.MoveTo(position, lobbyModel.menuTransitionTime, 0f, EaseType.easeOutExpo);

            cancelDeck.Dispatch();
        }
    }
}


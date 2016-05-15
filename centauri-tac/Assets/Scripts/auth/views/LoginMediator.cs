using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class LoginMediator : Mediator
    {
        [Inject]
        public LoginView view { get; set; }

        [Inject]
        public NeedLoginSignal needLoginSignal { get; set; }

        [Inject]
        public TryLoginSignal loginSignal { get; set; }

        [Inject]
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public AuthLoggedInSignal loggedInSignal { get; set; }

        [Inject]
        public PlayerFetchedSignal playerFetched { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLoginClicked);

            needLoginSignal.AddListener(onNeedLogin);
            failedAuth.AddListener(onFailAuth);
            loggedInSignal.AddListener(onLoggedIn);
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onLoginClicked);
        }

        public void onFailAuth()
        {
            view.onBadPassword();
        }

        private void onLoginClicked()
        {
            loginSignal.Dispatch(new Credentials() { username = view.email.text, password = view.password.text });
        }

        private void onNeedLogin()
        {
            view.enabled = true;
            view.gameObject.SetActive(true);
            view.init();
        }

        private void onLoggedIn(LoginStatusModel status, SocketKey key)
        {
            view.enabled = false;
            view.gameObject.SetActive(false);

            needLoginSignal.RemoveListener(onNeedLogin);
            failedAuth.RemoveListener(onFailAuth);
            loggedInSignal.RemoveListener(onLoggedIn);


            //start checking for successful login, if one is not found (by the player fetched) 
            //resend the login signal to retrigger the fetch player command
            loginStatus = status;
            this.key = key;
            checkingLoginSuccess = true;
        }

        private void onPlayerFetched(PlayerModel p, SocketKey key)
        {
            checkingLoginSuccess = false;
        }

        private bool checkingLoginSuccess = false;
        private float loginTimer = 0f;
        private const float retryTime = 1f;
        private int retries = 0;

        private LoginStatusModel loginStatus;
        private SocketKey key;

        void Update()
        {
            if(!checkingLoginSuccess) return;

            loginTimer += Time.deltaTime;

            if (loginTimer > retryTime)
            {
                loginTimer = 0f;
                retries++;

                debug.Log("Retrying fetching player");
                loggedInSignal.Dispatch(loginStatus, key);
            }

            if (retries > 5)
            {
                debug.Log("Couldn't fetch player after 5 retries");
                checkingLoginSuccess = false;
            }
        }
    }
}


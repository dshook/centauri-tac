using ctac.signals;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ctac
{
    //Makes sure we connect in the beginning and (eventually) stay connected
    public class LoginWatcherView : View
    {

        [Inject] public AuthLoggedInSignal loggedInSignal { get; set; }
        [Inject] public IDebugService debug { get; set; }
        [Inject] public PlayerFetchedSignal playerFetched { get; set; }

        private bool checkingLoginSuccess = false;
        private float loginTimer = 0f;
        private const float retryTime = 1f;
        private int retries = 0;

        private LoginStatusModel loginStatus;
        private SocketKey key;

        new void Start()
        {
            loggedInSignal.AddListener(onLoggedIn);
            playerFetched.AddListener(onPlayerFetched);
        }

        private void onLoggedIn(LoginStatusModel status, SocketKey key)
        {
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


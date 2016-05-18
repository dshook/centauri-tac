using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    //Makes sure we connect in the beginning and (eventually) stay connected
    public class LoginWatcherView : View
    {
        [Inject] public AuthLoggedInSignal loggedInSignal { get; set; }
        [Inject] public IDebugService debug { get; set; }
        [Inject] public PlayerFetchedSignal playerFetched { get; set; }

        private float loginTimer = 0f;
        private const float retryTime = 1f;
        private int retries = 0;

        private class LoginWatch
        {
            public LoginStatusModel loginStatus { get; set; }
            public bool checkSuccess { get; set; }
        }

        private Dictionary<SocketKey, LoginWatch> loginStatuses = new Dictionary<SocketKey, LoginWatch>();

        new void Start()
        {
            loggedInSignal.AddListener(onLoggedIn);
            playerFetched.AddListener(onPlayerFetched);
        }

        private void onLoggedIn(LoginStatusModel status, SocketKey key)
        {
            //start checking for successful login, if one is not found (by the player fetched) 
            //resend the login signal to retrigger the fetch player command
            loginStatuses[key] = new LoginWatch()
            {
                loginStatus = status,
                checkSuccess = true
            };
        }

        private void onPlayerFetched(PlayerModel p, SocketKey key)
        {
            if (!loginStatuses.ContainsKey(key))
            {
                debug.LogWarning("Key not found in check login statuses");
                return;
            }
            loginStatuses[key].checkSuccess = false;
        }

        void Update()
        {
            if(!loginStatuses.Any(v => v.Value.checkSuccess)) return;

            loginTimer += Time.deltaTime;

            if (loginTimer > retryTime)
            {
                loginTimer = 0f;
                retries++;

                debug.Log("Retrying fetching player");
                var loginToRetry = loginStatuses.First(v => v.Value.checkSuccess);
                loggedInSignal.Dispatch(loginToRetry.Value.loginStatus, loginToRetry.Key);
            }

            if (retries > 5)
            {
                debug.Log("Couldn't fetch player after 5 retries");
            }
        }
    }
}


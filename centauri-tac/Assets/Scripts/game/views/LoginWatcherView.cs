using ctac.signals;
using Newtonsoft.Json;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ctac
{
    //Makes sure we connect in the beginning and (eventually) stay connected
    public class LoginWatcherView : View
    {
        [Inject] public IDebugService debug { get; set; }

        [Inject] public AuthLoggedInSignal loggedInSignal { get; set; }
        [Inject] public TryLoginSignal tryLoginSignal { get; set; }

        private float loginTimer = 0f;
        private const float retryTime = 2f;
        private int retries = 0;

        private class LoginWatch
        {
            public LoginStatusModel loginStatus { get; set; }
            public bool checkSuccess { get; set; }
        }

        private Dictionary<SocketKey, LoginWatch> loginStatuses = new Dictionary<SocketKey, LoginWatch>();
        private Dictionary<Credentials, bool> tryLoginStatuses = new Dictionary<Credentials, bool>();

        new void Start()
        {
        }

        private void onTryLogin(Credentials c)
        {
            tryLoginStatuses[c] = true;
        }

        [ListensTo(typeof(TokenSignal))]
        public void tokenReceived(string token, SocketKey key)
        {
            //decode and mark try login statuses to stop checking
            var splitToken = token.Split('.');
            if (splitToken.Length != 3) {
                debug.LogError("Login token appears invalid " + token, key);
                return;
            }
            var payload = splitToken[1];

            //you have to pad base 64 strings with ='s so the length is a multiple of 4
            string dummyData = payload.Trim().Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');

            byte[] data = Convert.FromBase64String(dummyData);
            var jsonWebToken = Encoding.UTF8.GetString(data);
            var payloadObj = JsonConvert.DeserializeObject<TokenPayload>(jsonWebToken);

            if (payloadObj == null || payloadObj.sub == null || string.IsNullOrEmpty(payloadObj.sub.email))
            {
                debug.LogError("Could not get login info from token " + jsonWebToken, key);
                return;
            }

            //find right try login status to mark
            var loginStatus = tryLoginStatuses.Where(x => x.Key.username == payloadObj.sub.email).FirstOrDefault();
            if (loginStatus.Key != null)
            {
                tryLoginStatuses[loginStatus.Key] = false;
            }
        }

        [ListensTo(typeof(LoggedInSignal))]
        public void onLoggedIn(LoginStatusModel status, SocketKey key)
        {
            //start checking for successful login, if one is not found (by the player fetched) 
            //resend the login signal to retrigger the fetch player command
            loginStatuses[key] = new LoginWatch()
            {
                loginStatus = status,
                checkSuccess = true
            };
        }

        [ListensTo(typeof(PlayerFetchedSignal))]
        public void onPlayerFetched(PlayerModel p, SocketKey key)
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
            var checkLogin = loginStatuses.Any(v => v.Value.checkSuccess);
            var tryLogin = tryLoginStatuses.Any(v => v.Value);
            if(!checkLogin && !tryLogin) return;

            loginTimer += Time.deltaTime;

            if (loginTimer > retryTime)
            {
                loginTimer = 0f;
                retries++;

                if (tryLogin)
                {
                    debug.Log("Retrying logging in");
                    var loginToRetry = tryLoginStatuses.First(v => v.Value);
                    tryLoginSignal.Dispatch(loginToRetry.Key);
                }

                if (checkLogin)
                {
                    debug.Log("Retrying fetching player");
                    var loginToRetry = loginStatuses.First(v => v.Value.checkSuccess);
                    loggedInSignal.Dispatch(loginToRetry.Value.loginStatus, loginToRetry.Key);
                }
            }

            if (retries > 5)
            {
                debug.Log("Couldn't fetch player after 5 retries");
            }
        }



        public class Sub
        {
            public int id { get; set; }
            public string email { get; set; }
        }

        public class TokenPayload
        {
            public Sub sub { get; set; }
            public List<string> roles { get; set; }
            public int iat { get; set; }
            public int exp { get; set; }
        }
    }
}


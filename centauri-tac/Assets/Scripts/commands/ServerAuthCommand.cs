/// The only change in StartCommand is that we extend Command, not EventCommand
using UnityEngine;
using strange.extensions.command.impl;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public class ServerAuthCommand : Command
    {
        [Inject]
        public NeedLoginSignal needLoginSignal { get; set; }

        [Inject]
        public TryLoginSignal loginSignal { get; set; }

        [Inject]
        public LoggedInSignal loggedInSignal { get; set; }

        [Inject]
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public IAuthModel authModel { get; set; }

        [Inject]
        public IJsonNetworkService netService { get; set; }

        const string playerTokenKey = "playerToken";

        public override void Execute()
        {
            Retain();
            loginSignal.AddListener(setCredentials);

            //determine if we need to authenticate with the server to fetch a token
            var playerToken = PlayerPrefs.GetString(playerTokenKey);
            if (!string.IsNullOrEmpty(playerToken))
            {
                authModel.token = playerToken;
                netService.fulfillSignal.AddListener(onLoginComplete);
                netService.Request("auth", "player/me", authModel.GetType());
            }
            else
            {
                needLoginSignal.Dispatch();
            }
        }

        private void setCredentials(string user, string password)
        {
            netService.fulfillSignal.AddListener(onLoginComplete);
            netService.Request("auth", "player/login", authModel.GetType(), 
                new Dictionary<string, string>() {
                    { "email" , user },
                    { "password", password }
                }
            );
        }

        private void onLoginComplete(string url, object data)
        {
            netService.fulfillSignal.RemoveListener(onLoginComplete);
            if (data == null)
            {
                Debug.LogError("Failed Authenticate");
                failedAuth.Dispatch();
                needLoginSignal.Dispatch();
            }
            else
            {
                Debug.Log("Authenticated");
                authModel = data as AuthModel;
                PlayerPrefs.SetString(playerTokenKey, authModel.token);

                loginSignal.RemoveListener(setCredentials);

                loggedInSignal.Dispatch(authModel);
            }

            Release();
        }
    }
}


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
        public TryLoginSignal tryLoginSignal { get; set; }

        [Inject]
        public TokenSignal tokenSignal { get; set; }

        [Inject]
        public LoggedInSignal loggedInSignal { get; set; }

        [Inject]
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public AuthModel authModel { get; set; }

        [Inject]
        public PlayerModel playerModel { get; set; }

        [Inject(name = InjectKeys.authSocketService)]
        public ISocketService socketService { get; set; }

        const string playerTokenKey = "playerToken";

        public override void Execute()
        {
            Retain();

            //determine if we need to authenticate with the server to fetch a token
            var playerToken = PlayerPrefs.GetString(playerTokenKey);
            if (!string.IsNullOrEmpty(playerToken))
            {
                authModel.token = playerToken;
                sendAuthToken();
                Release();
            }
            else
            {
                tryLoginSignal.AddListener(setCredentials);
                tokenSignal.AddListener(onTokenComplete);
                needLoginSignal.Dispatch();
            }
        }

        private void setCredentials(string user, string password)
        {
            socketService.Request("auth", "login", 
                new {
                    email = user,
                    password = password
                }
            );
        }

        private void sendAuthToken()
        {
            socketService.Request("auth", "token", authModel.token );
        }

        private void onTokenComplete(string token)
        {
            if (data == null)
            {
                Debug.LogError("Failed Authenticate");
                failedAuth.Dispatch();
                needLoginSignal.Dispatch();
            }
            else
            {
                Debug.Log("Authenticated");
                authModel.token = token;
                PlayerPrefs.SetString(playerTokenKey, authModel.token);

                tokenSignal.RemoveListener(onTokenComplete);
                tryLoginSignal.RemoveListener(setCredentials);
                Release();
            }

        }
    }
}


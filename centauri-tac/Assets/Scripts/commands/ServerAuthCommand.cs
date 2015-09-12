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
        public TryLoginSignal loginSignal { get; set; }

        [Inject]
        public LoggedInSignal loggedInSignal { get; set; }

        [Inject]
        public IAuthModel authModel { get; set; }

        [Inject]
        public IJsonNetworkService netService { get; set; }

        public override void Execute()
        {
            Retain();
            loginSignal.AddListener(setCredentials);
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
            }
            else
            {
                Debug.Log("Authenticated");
                authModel = data as AuthModel;

                loggedInSignal.Dispatch(authModel);
            }

            Release();
        }
    }
}


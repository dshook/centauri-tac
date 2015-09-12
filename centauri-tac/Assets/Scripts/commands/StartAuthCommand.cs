/// The only change in StartCommand is that we extend Command, not EventCommand
using UnityEngine;
using strange.extensions.command.impl;
using System.Collections.Generic;

namespace ctac
{
    public class StartAuthCommand : Command
    {
        [Inject]
        public IAuthModel authModel { get; set; }

        [Inject]
        public IJsonNetworkService netService { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; }

        [Inject]
        public IConfigModel configModel { get; set; }

        public override void Execute()
        {
            Retain();
            netService.fulfillSignal.AddListener(onConnectComplete);
            netService.Request("master", "components", componentModel.componentList.GetType() );

        }

        //Connect to get list of components
        private void onConnectComplete(string url, object data)
        {
            netService.fulfillSignal.RemoveListener(onConnectComplete);
            if (data == null)
            {
                Debug.LogError("Failed to Connect");
            }
            else
            {
                Debug.Log("Connected");
                componentModel.componentList = data as List<Component>;
            }

            //rebind listener for the next step. lil nasty but better than creating another signal imo
            netService.fulfillSignal.AddListener(onLoginComplete);
            netService.Request("auth", "player/login", authModel.GetType(), 
                new Dictionary<string, string>() {
                    { "email" , configModel.email },
                    { "password", configModel.password }
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
            }

            Release();
        }
    }
}


/// The only change in StartCommand is that we extend Command, not EventCommand
using UnityEngine;
using strange.extensions.command.impl;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public class ServerConnnectCommand : Command
    {
        [Inject]
        public ConnectedSignal connectedSignal { get; set; }

        [Inject]
        public IJsonNetworkService netService { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; }

        public override void Execute()
        {
            Retain();
            netService.fulfillSignal.AddListener(onConnectComplete);
            netService.Request("master", "components", componentModel.componentList.GetType());
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
                connectedSignal.Dispatch();
            }

            Release();
        }
    }
}


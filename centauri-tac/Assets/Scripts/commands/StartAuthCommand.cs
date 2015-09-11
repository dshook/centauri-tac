/// The only change in StartCommand is that we extend Command, not EventCommand
using UnityEngine;
using strange.extensions.command.impl;

namespace ctac
{
    public class StartAuthCommand : Command
    {
        [Inject]
        public IAuthModel model { get; set; }

        [Inject]
        public IJsonNetworkService netService { get; set; }

        [Inject]
        public IComponentModel componentModel { get; set; }

        public override void Execute()
        {
            Retain();
            netService.fulfillSignal.AddListener(onComplete);
            netService.Request("master", "component/owned", componentModel.componentList.GetType() );

        }

        //The payload is now a type-safe string
        private void onComplete(string url, object data)
        {
            netService.fulfillSignal.RemoveListener(onComplete);

            model.data = data;

            Debug.Log("Auth request finished");

            Release();
        }
    }
}


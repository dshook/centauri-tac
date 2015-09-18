/// The only change in StartCommand is that we extend Command, not EventCommand
using UnityEngine;
using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class FetchPlayerCommand : Command
    {
        [Inject]
        public PlayerFetchedSignal playerFetchedSignal { get; set; }

        [Inject]
        public AuthModel authModel { get; set; }

        [Inject]
        public PlayerModel playerModel { get; set; }

        [Inject]
        public IJsonNetworkService netService { get; set; }

        public override void Execute()
        {
            Retain();
            netService.fulfillSignal.AddListener(onFetchComplete);
            netService.Request("auth", "player/me", playerModel.GetType());
        }

        private void onFetchComplete(string url, object data)
        {
            netService.fulfillSignal.RemoveListener(onFetchComplete);
            if (data == null)
            {
                Debug.LogError("Failed Fetching Player");
            }
            else
            {
                Debug.Log("Player Fetched");
                playerModel = data as PlayerModel;

                playerFetchedSignal.Dispatch();
            }

            Release();
        }
    }
}


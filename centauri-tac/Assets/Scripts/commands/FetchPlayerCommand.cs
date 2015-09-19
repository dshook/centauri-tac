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

        [Inject(name = InjectKeys.authSocketService)]
        public ISocketService socketService { get; set; }

        public override void Execute()
        {
            Retain();
            playerFetchedSignal.AddListener(onFetchComplete);
            socketService.Request("auth", "me");
        }

        private void onFetchComplete(PlayerModel player)
        {
            playerFetchedSignal.RemoveListener(onFetchComplete);
            if (player == null)
            {
                Debug.LogError("Failed Fetching Player");
            }
            else
            {
                Debug.Log("Player Fetched");
                playerModel = player;
            }

            Release();
        }
    }
}


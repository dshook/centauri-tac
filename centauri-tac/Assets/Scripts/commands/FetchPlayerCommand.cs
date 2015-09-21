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
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        public override void Execute()
        {
            Retain();
            playerFetchedSignal.AddListener(onFetchComplete);
            SocketKey loggedInKey = ((object[])data)[1] as SocketKey;
            socketService.Request(loggedInKey.clientId, "auth", "me");
        }

        private void onFetchComplete(PlayerModel player, SocketKey key)
        {
            playerFetchedSignal.RemoveListener(onFetchComplete);
            if (player == null)
            {
                Debug.LogError("Failed Fetching Player");
            }
            else
            {
                Debug.Log("Player Fetched");
                var playerModel = playersModel.GetByClientId(key.clientId);
                //kinda nasty save of the couple properties that need to be saved on the original player model
                //that won't be coming across the wire
                var clientId = playerModel.clientId;
                var token = playerModel.token;
                player.CopyProperties(playerModel);

                playerModel.clientId = clientId;
                playerModel.token = token;
                //playersModel.players.Add(player);
            }

            Release();
        }
    }
}


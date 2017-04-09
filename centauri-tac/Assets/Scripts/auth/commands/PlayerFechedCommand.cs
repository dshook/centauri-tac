using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class PlayerFetchedCommand : Command
    {
        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public PlayerModel player { get; set; }

        [Inject]
        public PlayerFetchedFinishedSignal finished { get; set; }

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            onFetchComplete(player, key);
        }

        private void onFetchComplete(PlayerModel player, SocketKey key)
        {
            if (player == null)
            {
                debug.LogError("Failed Fetching Player", key);
            }
            else
            {
                debug.Log("Player Fetched", key);
                var playerModel = playersModel.GetByClientId(key.clientId);
                //kinda nasty save of the couple properties that need to be saved on the original player model
                //that won't be coming across the wire
                var clientId = playerModel.clientId;
                var token = playerModel.token;
                player.CopyProperties(playerModel);

                playerModel.clientId = clientId;
                playerModel.token = token;
                playerModel.isLocal = true;

                socketService.Disconnect(key.clientId, "auth");
                finished.Dispatch(playerModel, key);
            }
        }
    }
}


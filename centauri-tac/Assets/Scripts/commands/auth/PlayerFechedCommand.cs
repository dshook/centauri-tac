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

        public override void Execute()
        {
            var objectData = ((object[])data);
            var player = objectData[0] as PlayerModel;
            var key = objectData[1] as SocketKey;
            onFetchComplete(player, key);
        }

        private void onFetchComplete(PlayerModel player, SocketKey key)
        {
            if (player == null)
            {
                debug.LogError("Failed Fetching Player");
            }
            else
            {
                debug.Log("Player Fetched");
                var playerModel = playersModel.GetByClientId(key.clientId);
                //kinda nasty save of the couple properties that need to be saved on the original player model
                //that won't be coming across the wire
                var clientId = playerModel.clientId;
                var token = playerModel.token;
                player.CopyProperties(playerModel);

                playerModel.clientId = clientId;
                playerModel.token = token;
            }
        }
    }
}


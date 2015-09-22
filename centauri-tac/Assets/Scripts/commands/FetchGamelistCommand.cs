using strange.extensions.command.impl;

namespace ctac
{
    public class FetchGamelistCommand : Command
    {
        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        public override void Execute()
        {
            var objectData = ((object[])data);
            var player = objectData[0] as PlayerModel;
            var key = objectData[1] as SocketKey;
            onFetchComplete(player, key);
        }

        private void onFetchComplete(PlayerModel player, SocketKey key)
        {
            socketService.Request(key.clientId, "gamelist", "gamelist");
        }
    }
}


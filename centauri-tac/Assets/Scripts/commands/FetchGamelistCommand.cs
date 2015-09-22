using strange.extensions.command.impl;

namespace ctac
{
    public class FetchGamelistCommand : Command
    {
        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public PlayerModel player { get; set; }

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            onFetchComplete(player, key);
        }

        private void onFetchComplete(PlayerModel player, SocketKey key)
        {
            socketService.Request(key.clientId, "gamelist", "gamelist");
        }
    }
}


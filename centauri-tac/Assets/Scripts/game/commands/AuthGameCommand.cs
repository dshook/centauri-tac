using strange.extensions.command.impl;

namespace ctac
{
    public class AuthGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public GamelistModel gamelist { get; set; }

        [Inject]
        public GameModel game { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            //update the game list so once we're authed we can find and join it
            game.isCurrent = true;
            gamelist.AddOrUpdateGame(socketKey.clientId, game);
            debug.Log(socketKey.clientId.ToShort() + " current game " + game.id);

            var player = playersModel.GetByClientId(socketKey.clientId);
            socket.Request(socketKey.clientId, "game", "token", player.token, game.component.wsURL);
        }
    }
}


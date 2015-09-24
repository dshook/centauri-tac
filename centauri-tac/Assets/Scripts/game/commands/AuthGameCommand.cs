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
        public ComponentModel components { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public GamelistModel gamelist { get; set; }

        [Inject]
        public GameMetaModel game { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            if (game == null)
            {
                return;
            }
            socket.Disconnect(socketKey.clientId, "matchmaker");
            //update the game list so once we're authed we can find and join it
            game.isCurrent = true;
            gamelist.AddOrUpdateGame(socketKey.clientId, game);
            debug.Log(socketKey.clientId.ToShort() + " current game " + game.id);

            //add game to list of components for use
            components.componentList.Add(game.component);

            var player = playersModel.GetByClientId(socketKey.clientId);
            socket.Request(socketKey.clientId, "game", "token", player.token);
        }
    }
}


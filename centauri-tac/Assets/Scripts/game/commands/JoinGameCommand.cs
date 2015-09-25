using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class JoinGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public GamelistModel gamelist { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            var myGames = gamelist.games.Get(socketKey.clientId);
            var gameToJoin = myGames.FirstOrDefault(x => x.isCurrent);
            if (gameToJoin != null)
            {
                debug.Log("Joining game " + gameToJoin.id, socketKey);
                socket.Request(socketKey, "join", gameToJoin.id);
            }
        }
    }
}


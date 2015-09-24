using strange.extensions.command.impl;
using System;
using System.Linq;

namespace ctac
{
    public class GamelistGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public ComponentModel components { get; set; }

        [Inject]
        public GamelistModel gamelist { get; set; }

        [Inject]
        public GameModel game { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            gamelist.AddOrUpdateGame(socketKey.clientId, game);
            debug.Log(socketKey.clientId.ToShort() + " gamelist " + gamelist.GamesToString(socketKey.clientId));

            if (!components.componentList.Any(x => x.typeId == game.component.typeId))
            {
                components.componentList.Add(game.component);
            }
        }
    }
}


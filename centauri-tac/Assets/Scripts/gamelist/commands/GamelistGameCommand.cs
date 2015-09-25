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
        public GameMetaModel game { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            gamelist.AddOrUpdateGame(socketKey.clientId, game);
            debug.Log("Gamelist " + gamelist.GamesToString(socketKey.clientId), socketKey);

            if (!components.componentList.Any(x => x.typeId == game.component.typeId))
            {
                components.componentList.Add(game.component);
            }
        }
    }
}


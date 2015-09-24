using ctac.signals;
using strange.extensions.command.impl;
using System;
using System.Linq;

namespace ctac
{
    public class PlayerPartCommand : Command
    {
        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public GameJoinConnectModel playerParted { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            gamePlayers.players.Remove(gamePlayers.players.FirstOrDefault(x => x.id == playerParted.id));
        }
    }
}


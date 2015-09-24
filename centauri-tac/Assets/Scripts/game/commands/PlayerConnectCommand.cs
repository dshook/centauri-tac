using ctac.signals;
using strange.extensions.command.impl;
using System;
using System.Linq;

namespace ctac
{
    public class PlayerConnectCommand : Command
    {
        [Inject]
        public PlayersModel players { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public GameJoinConnectModel playerConnected { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            var player = players.players.FirstOrDefault(x => x.id == playerConnected.id);
            if (player != null)
            {
                //this must be a local player so just grab them
                gamePlayers.players.Add(player);
            }
            else
            {
                //remote players need a new player obj created
                gamePlayers.players.Add(new PlayerModel()
                {
                    clientId = Guid.NewGuid(),
                    isLocal = false,
                    id = playerConnected.id,
                    email = playerConnected.email,
                    registered = playerConnected.registered
                });
            }
        }
    }
}


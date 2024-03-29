using strange.extensions.command.impl;
using System;
using System.Linq;

namespace ctac
{
    public class PlayerConnectCommand : Command
    {
        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public PlayersModel players { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public JoinOrConnectModel playerConnected { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            var player = players.players.FirstOrDefault(x => x.id == playerConnected.id);
            if (player != null)
            {
                //this must be a local player so just grab them
                gamePlayers.AddOrUpdate(player);
                gamePlayers.SetMeClient(player.clientId);
            }
            else
            {
                //remote players need a new player obj created
                gamePlayers.AddOrUpdate(new PlayerModel()
                {
                    clientId = Guid.NewGuid(),
                    isLocal = false,
                    id = playerConnected.id,
                    email = playerConnected.email,
                    registered = playerConnected.registered
                });
            }

            debug.Log("Player Joined " + playerConnected.email + " Total Players " + gamePlayers.players.Count, socketKey);

        }
    }
}


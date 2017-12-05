using UnityEngine;
using strange.extensions.command.impl;
using ctac.signals;
using System;

namespace ctac
{
    public class ServerAuthCommand : Command
    {
        [Inject]
        public NeedLoginSignal needLoginSignal { get; set; }

        [Inject]
        public TokenSignal tokenSignal { get; set; }

        [Inject]
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public ConfigModel config { get; set; }

        public override void Execute()
        {
            //determine if we need to authenticate with the server to fetch a token
            var playerToken = PlayerPrefs.GetString(CtacConst.playerToken);
            if (!string.IsNullOrEmpty(playerToken))
            {
                var newPlayer = new PlayerModel()
                {
                    clientId = Guid.NewGuid(),
                    token = playerToken
                };
                playersModel.players.Add(newPlayer);
                sendAuthToken(newPlayer);
            }
            else
            {
                needLoginSignal.Dispatch(null);
            }
        }

        private void sendAuthToken(PlayerModel playerModel)
        {
            socketService.Request(playerModel.clientId, "auth", "token", playerModel.token );
        }

    }
}


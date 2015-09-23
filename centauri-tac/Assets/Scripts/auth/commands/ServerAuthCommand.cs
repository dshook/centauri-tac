/// The only change in StartCommand is that we extend Command, not EventCommand
using UnityEngine;
using strange.extensions.command.impl;
using System.Collections.Generic;
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
        public TryLoginSignal tryLoginSignal { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public ConfigModel config { get; set; }

        public override void Execute()
        {
#if DEBUG
            //authenticate players from config if needed
            if (config.players.Count > 0)
            {
                foreach (var player in config.players)
                {
                    tryLoginSignal.Dispatch(new Credentials() { username = player.username, password = player.password });
                }
                return;
            }
#endif
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
                needLoginSignal.Dispatch();
            }
        }

        private void sendAuthToken(PlayerModel playerModel)
        {
            socketService.Request(playerModel.clientId, "auth", "token", playerModel.token );
        }

    }
}


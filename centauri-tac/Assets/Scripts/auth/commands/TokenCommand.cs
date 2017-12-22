using strange.extensions.command.impl;
using ctac.signals;
using System;
using UnityEngine;

namespace ctac
{
    public class TokenCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public string token { get; set; }

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            onTokenComplete(token, key);
        }

        private void onTokenComplete(string token, SocketKey key)
        {
            if (string.IsNullOrEmpty(token))
            {
                debug.LogError("Failed Authenticate", key);
                failedAuth.Dispatch();
            }
            else
            {
                debug.Log("Authenticated", key);
                var player = playersModel.GetByClientId(key.clientId);
                if (player == null)
                {
                    playersModel.players.Add(new PlayerModel()
                    {
                        clientId = key.clientId,
                        token = token
                    });
                }
                PlayerPrefs.SetString(Constants.playerToken, token);

            }

        }
    }
}


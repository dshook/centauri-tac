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
        public NeedLoginSignal needLoginSignal { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; }

        public override void Execute()
        {
            var dataArray = (object[])data;
            var token = (string)dataArray[0];
            var key  = dataArray[1] as SocketKey;

            onTokenComplete(token, key);
        }

        private void onTokenComplete(string token, SocketKey key)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Failed Authenticate");
                failedAuth.Dispatch();
                needLoginSignal.Dispatch();
            }
            else
            {
                Debug.Log("Authenticated");
                var player = playersModel.GetByClientId(key.clientId);
                if (player == null)
                {
                    playersModel.players.Add(new PlayerModel()
                    {
                        clientId = key.clientId,
                        token = token
                    });
                }
                PlayerPrefs.SetString(CtacConst.playerToken, token);

            }

        }
    }
}


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
        public TryLoginSignal tryLoginSignal { get; set; }

        [Inject]
        public TokenSignal tokenSignal { get; set; }

        [Inject]
        public FailedAuthSignal failedAuth { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        const string playerTokenKey = "playerToken";

        public override void Execute()
        {
            Retain();

            //determine if we need to authenticate with the server to fetch a token
            var playerToken = PlayerPrefs.GetString(playerTokenKey);
            if (!string.IsNullOrEmpty(playerToken))
            {
                var newPlayer = new PlayerModel()
                {
                    clientId = Guid.NewGuid(),
                    token = playerToken
                };
                playersModel.players.Add(newPlayer);
                sendAuthToken(newPlayer);
                Release();
            }
            else
            {
                tryLoginSignal.AddListener(setCredentials);
                tokenSignal.AddListener(onTokenComplete);
                needLoginSignal.Dispatch();
            }
        }

        private void setCredentials(string user, string password)
        {
            socketService.Request(Guid.NewGuid(), "auth", "login", 
                new {
                    email = user,
                    password = password
                }
            );
        }

        private void sendAuthToken(PlayerModel playerModel)
        {
            socketService.Request(playerModel.clientId, "auth", "token", playerModel.token );
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
                PlayerPrefs.SetString(playerTokenKey, token);

                tokenSignal.RemoveListener(onTokenComplete);
                tryLoginSignal.RemoveListener(setCredentials);
                Release();
            }

        }
    }
}


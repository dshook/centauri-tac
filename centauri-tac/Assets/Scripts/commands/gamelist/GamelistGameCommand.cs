using strange.extensions.command.impl;
using System;

namespace ctac
{
    public class GamelistGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GamelistModel gamelist { get; set; }

        public override void Execute()
        {
            var dataArray = (object[])data;
            var game = dataArray[0] as GamelistGameModel;
            var socketKey = dataArray[1] as SocketKey;

            updateGamelist(game, socketKey);
        }

        private void updateGamelist(GamelistGameModel game, SocketKey key)
        {
            var existingGame = gamelist.games.Get(key.clientId);
            if (existingGame != null)
            {
                game.CopyProperties(existingGame);
            }
            else
            {
                gamelist.games.Add(key.clientId, game);
            }
        }
    }
}


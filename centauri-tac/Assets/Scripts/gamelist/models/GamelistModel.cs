using System;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class GamelistModel
    {
        //gamelist per client
        public Dictionary<Guid, List<GamelistGameModel>> games = new Dictionary<Guid, List<GamelistGameModel>>();

        public void AddOrUpdateGame(Guid clientId, GamelistGameModel game)
        {
            var existingGames = games.Get(clientId);
            if (existingGames == null)
            {
                existingGames = new List<GamelistGameModel>();
                games[clientId] = existingGames;
            }

            var existingGame = existingGames.FirstOrDefault(x => x.id == game.id);
            if (existingGame != null)
            {
                game.CopyProperties(existingGame);
            }
            else
            {
                existingGames.Add(game);
            }
        }

        public string GamesToString(Guid clientId)
        {
            var existingGames = games.Get(clientId);
            if (existingGames == null || existingGames.Count == 0)
            {
                return "none";
            }
            return string.Join(",", existingGames.Select(x => x.id.ToString()).ToArray());
        }
    }
}

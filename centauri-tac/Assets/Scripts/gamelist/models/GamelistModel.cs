using System;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class GamelistModel
    {
        //gamelist per client
        public Dictionary<Guid, List<GameModel>> games = new Dictionary<Guid, List<GameModel>>();

        public void AddOrUpdateGame(Guid clientId, GameModel game)
        {
            var existingGames = games.Get(clientId);
            if (existingGames == null)
            {
                existingGames = new List<GameModel>();
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

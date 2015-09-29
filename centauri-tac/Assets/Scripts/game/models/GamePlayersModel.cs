using System;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    /// <summary>
    /// Players connected to the current game
    /// </summary>
    [Singleton]
    public class GamePlayersModel
    {
        public List<PlayerModel> players = new List<PlayerModel>();

        public PlayerModel GetByClientId(Guid clientId)
        {
            return players.Where(x => x.clientId == clientId).FirstOrDefault();
        }

        public void AddOrUpdate(PlayerModel player)
        {
            var existing = players.FirstOrDefault(x => x.id == player.id);
            if (existing == null)
            {
                players.Add(player);
            }
            else
            {
                player.CopyProperties(existing);
            }
        }
    }
}

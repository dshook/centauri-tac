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

        public PlayerModel GetByPlayerId(int playerId)
        {
            return players.FirstOrDefault(x => x.id == playerId);
        }

        public int OpponentId(int currentTurnPlayerId)
        {
            PlayerModel opponent;
            //hotseat mode
            if (players.Count(x => x.isLocal) > 1)
            {
                opponent = players.FirstOrDefault(p => p.id != currentTurnPlayerId);
            }
            //net mode
            else
            {
                opponent = players.FirstOrDefault(p => !p.isLocal);
            }
            return opponent != null ? opponent.id : -1;
        }
    }
}

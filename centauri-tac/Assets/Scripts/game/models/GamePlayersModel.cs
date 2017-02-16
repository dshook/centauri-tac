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
        private PlayerModel mePlayer = null;

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

        public PlayerModel Me
        {
            get
            {
                return mePlayer;
            }
        }

        //ensures there's only one "me" client, should really only be neccessary for local hotseat testing
        public void SetMeClient(Guid clientId)
        {
            foreach (var p in players)
            {
                p.isMe = false;
            }
            
            var player = GetByClientId(clientId);
            player.isMe = true;
            mePlayer = player;
        }

        public PlayerModel GetByPlayerId(int playerId)
        {
            return players.FirstOrDefault(x => x.id == playerId);
        }

        public bool isHotseat
        {
            get
            {
                return players.Count(x => x.isLocal) > 1;
            }
        }

        public PlayerModel Opponent(int currentTurnPlayerId)
        {
            PlayerModel opponent;
            if (isHotseat)
            {
                opponent = players.FirstOrDefault(p => p.id != currentTurnPlayerId);
            }
            //net mode
            else
            {
                opponent = players.FirstOrDefault(p => !p.isLocal);
            }
            return opponent;
        }
    }
}

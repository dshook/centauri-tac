using System;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class PlayersModel
    {
        public List<PlayerModel> players = new List<PlayerModel>();

        public PlayerModel GetByClientId(Guid clientId)
        {
            return players.Where(x => x.clientId == clientId).FirstOrDefault();
        }
    }
}


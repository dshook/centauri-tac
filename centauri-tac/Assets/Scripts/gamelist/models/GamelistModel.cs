using System;
using System.Collections.Generic;

namespace ctac
{
    [Singleton]
    public class GamelistModel
    {
        //gamelist per client
        public Dictionary<Guid, GamelistGameModel> games = new Dictionary<Guid, GamelistGameModel>();
    }
}

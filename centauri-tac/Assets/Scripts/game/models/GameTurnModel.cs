using System;

namespace ctac
{
    [Singleton]
    public class GameTurnModel
    {
        public Guid currentTurnClientId { get; set; }
    }
}

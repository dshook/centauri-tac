using System;

namespace ctac
{
    [Singleton]
    public class GameTurnModel
    {
        public int currentTurn { get; set; }
        public Guid currentTurnClientId { get; set; }
    }
}

using System;

namespace ctac
{
    [Singleton]
    public class GameTurnModel
    {
        public int currentTurn { get; set; }

        //client hack to indicate if this is the client switching sides in dev mode
        public bool isClientSwitch { get; set; }
    }
}

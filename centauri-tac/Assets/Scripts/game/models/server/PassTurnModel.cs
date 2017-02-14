using System.Collections.Generic;

namespace ctac
{
    public class PassTurnModel : BaseAction
    {
        public int currentTurn { get; set; }

        public List<TurnResourceModel> playerResources { get; set; }
    }

    public class TurnResourceModel
    {
        public int playerId { get; set; }
        public int current { get; set; }
        public int max { get; set; }
    }
}

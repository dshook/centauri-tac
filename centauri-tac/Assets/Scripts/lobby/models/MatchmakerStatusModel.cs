namespace ctac
{
    public class MatchmakerStatusModel
    {
        public int playerId { get; set; }
        public bool inQueue { get; set; }
        public bool beingMatched { get; set; }
    }
}

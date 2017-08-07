namespace ctac
{
    public class GameMetaModel
    {
        public bool isCurrent { get; set; }

        public int id { get; set; }
        public string name { get; set; }
        public string map { get; set; }
        public int maxPlayerCount { get; set; }
        public int hostPlayerId { get; set; }
        public HostPlayer hostPlayer { get; set; }
        public string registered { get; set; }
        public int stateId { get; set; }
        public int turnLengthMs { get; set; }
        public int turnEndBufferLengthMs { get; set; }
        public int turnIncrementLengthMs { get; set; }
        public GamelistGameState state { get; set; }
    }

    public class HostPlayer
    {
        public string email { get; set; }
        public int id { get; set; }
        public string registered { get; set; }
        public bool isAdmin { get; set; }
    }

    public class GamelistGameState
    {
        public int id { get; set; }
        public string name { get; set; }
    }

}

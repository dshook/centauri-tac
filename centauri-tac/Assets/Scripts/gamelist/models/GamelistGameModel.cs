namespace ctac
{
    public class GamelistGameModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int maxPlayerCount { get; set; }
        public int currentPlayerCount { get; set; }
        public int componentId { get; set; }
        public Component component { get; set; }
        public int hostPlayerId { get; set; }
        public HostPlayer hostPlayer { get; set; }
        public string registered { get; set; }
        public int stateId { get; set; }
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

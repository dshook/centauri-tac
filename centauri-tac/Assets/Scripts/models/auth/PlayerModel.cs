namespace ctac
{
    [Singleton]
    public class PlayerModel
    {
        public Player player { get; set; }
    }

    public class Player
    {
        public string email { get; set; }
        public int id { get; set; }
        public string registered { get; set; }
        public bool isAdmin { get; set; }
    }

}


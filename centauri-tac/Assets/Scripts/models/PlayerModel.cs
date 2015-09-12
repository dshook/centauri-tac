namespace ctac
{
    public interface IPlayerModel
    {
        Player player { get; set; }
    }

    public class PlayerModel : IPlayerModel
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


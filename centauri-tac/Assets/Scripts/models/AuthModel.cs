namespace ctac
{
    public interface IAuthModel
    {
        Player player { get; set; }
        string token { get; set; }
    }

    public class AuthModel : IAuthModel
    {
        public Player player { get; set; }
        public string token { get; set; }
    }

    public class Player
    {
        public string email { get; set; }
        public int id { get; set; }
        public string registered { get; set; }
        public bool isAdmin { get; set; }
    }

}


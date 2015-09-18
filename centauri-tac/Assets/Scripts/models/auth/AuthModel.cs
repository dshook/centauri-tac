namespace ctac
{
    public interface IAuthModel
    {
        string token { get; set; }
    }

    [Singleton]
    public class AuthModel : IAuthModel
    {
        public string token { get; set; }
    }
}


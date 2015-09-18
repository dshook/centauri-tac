namespace ctac
{
    public interface ILoginStatusModel
    {
        bool status { get; set; }
        string message { get; set; }
    }

    [Singleton]
    public class LoginStatusModel : ILoginStatusModel
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
}


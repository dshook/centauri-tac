namespace ctac
{
    public interface IAuthModel
    {
        object data { get; set; }
    }

    public class AuthModel : IAuthModel
    {
        public object data { get; set; }
    }
}


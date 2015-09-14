namespace ctac
{
    public interface IConfigModel
    {
        string baseUrl { get; }
        string realm { get; }
    }

    [Singleton]
    public class ConfigModel : IConfigModel
    {
        public string baseUrl { get { return "http://localhost:10123/"; } }
        public string realm { get { return "dshook"; } }
    }
}


namespace ctac
{
    public interface IConfigModel
    {
        string baseUrl { get; }
    }

    public class ConfigModel : IConfigModel
    {
        public string baseUrl { get { return "http://localhost:10123/"; } }
    }
}


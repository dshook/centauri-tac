namespace ctac
{
    [Singleton]
    public class ConfigModel
    {
        public string baseUrl { get { return "http://localhost:10123/"; } }
        public string realm { get { return "dshook"; } }
    }
}


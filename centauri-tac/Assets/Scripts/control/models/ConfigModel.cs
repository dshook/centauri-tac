using System.Collections.Generic;

namespace ctac
{
    [Singleton]
    public class ConfigModel
    {
        public string baseUrl = "http://localhost:10123/";

        public List<Credentials> players { get; set; }
    }
}


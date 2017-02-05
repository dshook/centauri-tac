using System.Collections.Generic;

namespace ctac
{
    [Singleton]
    public class ConfigModel
    {
        public string baseUrl { get; set; }

        public List<Credentials> players { get; set; }
    }
}


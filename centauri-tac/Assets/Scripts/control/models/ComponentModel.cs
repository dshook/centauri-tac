using System;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    //Holds the mapping of component to url
    public class ComponentModel
    {
        [Inject]
        public ConfigModel config { get; set; }

        public ComponentModel()
        {
        }

        //  Resolve component type into URL endpoint
        public string getComponentURL(string name)
        {
            return string.Format("{0}components/{1}/rest", config.baseUrl, name);
        }

        public string getComponentWSURL(string name)
        {
            return string.Format("{0}components/{1}", config.baseUrl, name).Replace("http://", "ws://");
        }
    }

    public class ComponentType
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool showInClient { get; set; }
        public bool enableREST { get; set; }
        public bool enableHTTP { get; set; }
        public bool enableWS { get; set; }
    }

    public class Component
    {
        public int id { get; set; }
        public int typeId { get; set; }
        public ComponentType type { get; set; }
        public string registered { get; set; }
        public string version { get; set; }
        public string realm { get; set; }
        public bool isActive { get; set; }
        public string httpURL { get; set; }
        public string restURL { get; set; }
        public string wsURL { get; set; }
    }
}


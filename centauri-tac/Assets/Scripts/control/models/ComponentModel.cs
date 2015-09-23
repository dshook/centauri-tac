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

        public List<Component> componentList { get; set; }

        public ComponentModel()
        {
            componentList = new List<Component>();
        }

        //  Resolve component type into URL endpoint
        public string getComponentURL(string name)
        {
            if (name == "master")
            {
                return config.baseUrl + "components/master/rest/realm/" + config.realm;
            }

            var component = componentList.Where(x => x.type.name == name).FirstOrDefault();

            if (component == null)
            {
                throw new Exception("no registered " + name + " components");
            }

            // TODO: handle multiple components being registered
            return component.httpURL + "/rest";
        }

        public string getComponentWSURL(string name)
        {
            var component = componentList.Where(x => x.type.name == name).FirstOrDefault();

            if (component == null)
            {
                throw new Exception("no registered " + name + " components");
            }

            return component.wsURL;
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


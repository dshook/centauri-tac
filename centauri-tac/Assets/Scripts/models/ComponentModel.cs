using System;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    //Holds the mapping of component to url
    public interface IComponentModel
    {
        List<Component> componentList { get; set; }
        string getComponentURL(string name);
    }

    public class ComponentModel : IComponentModel
    {
        [Inject]
        public IConfigModel config { get; set; }

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
            return component.url + "/rest";
        }
    }

    public class ComponentType
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Component
    {
        public string url { get; set; }
        public int id { get; set; }
        public int typeId { get; set; }
        public string registered { get; set; }
        public object lastPing { get; set; }
        public ComponentType type { get; set; }
    }
}


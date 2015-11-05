using System.Collections.Generic;

namespace ctac
{
    [Singleton]
    public class PlayerResourcesModel
    {
        //Dictionary from playerId to resource amount
        public Dictionary<int, int> resources = new Dictionary<int, int>();
    }
}

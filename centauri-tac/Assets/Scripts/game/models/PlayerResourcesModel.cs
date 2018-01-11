using System.Collections.Generic;

namespace ctac
{
    [GameSingleton]
    public class PlayerResourcesModel
    {
        //Dictionary from playerId to resource amount
        public Dictionary<int, int> resources = new Dictionary<int, int>();
        public Dictionary<int, int> maxResources = new Dictionary<int, int>();
    }
}

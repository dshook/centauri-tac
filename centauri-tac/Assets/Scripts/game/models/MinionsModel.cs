using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class MinionsModel
    {
        public List<MinionModel> minions { get; set; }

        public MinionModel Minion(int id)
        {
            return minions.FirstOrDefault(x => x.id == id);
        }
    }
}

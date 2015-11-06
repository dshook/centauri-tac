using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class CardDirectory
    {
        public List<CardModel> directory = new List<CardModel>();

        public CardModel Card(int id)
        {
            return directory.FirstOrDefault(x => x.id == id);
        }
    }
}

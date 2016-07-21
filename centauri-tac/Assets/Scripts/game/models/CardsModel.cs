using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class CardsModel
    {
        public List<CardModel> Cards { get; set; }

        public CardModel Card(int id)
        {
            return Cards.FirstOrDefault(x => x.id == id);
        }
    }
}

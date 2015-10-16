using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class CardsModel
    {
        public List<CardModel> Cards { get; set; }
    }
}

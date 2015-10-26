using System.Collections.Generic;

namespace ctac
{
    [Singleton]
    public class DecksModel
    {
        public List<CardModel> Cards { get; set; }
    }
}

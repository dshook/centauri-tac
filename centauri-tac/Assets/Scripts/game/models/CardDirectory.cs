using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class CardDirectory
    {
        public List<CardModel> directory = new List<CardModel>();

        public CardModel Card(int cardTemplateId)
        {
            return directory.FirstOrDefault(x => x.cardTemplateId == cardTemplateId);
        }
    }
}

using System.Collections.Generic;

namespace ctac
{
    //This deck model is for the card manager, not the in game deck
    public class DeckModel
    {
        public string name { get; set; }
        public List<CardModel> Cards = new List<CardModel>();
    }
}

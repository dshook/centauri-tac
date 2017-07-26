using Newtonsoft.Json;
using System.Collections.Generic;

namespace ctac
{
    //This deck model is for the card manager, not the in game deck
    public class DeckModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public Races race { get; set; }
        public List<CardModel> Cards = new List<CardModel>();

        [JsonIgnore]
        public DeckListView deckListView { get; set; }
    }
}

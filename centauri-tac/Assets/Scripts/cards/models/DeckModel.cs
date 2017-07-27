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
        public List<CardInDeckModel> cards = new List<CardInDeckModel>();

        [JsonIgnore]
        public DeckListView deckListView { get; set; }
    }

    //Very abbreviated form of a card in a deck that matches the server model
    public class CardInDeckModel
    {
        public int cardTemplateId { get; set; }
        public int quantity { get; set; }
    }
}

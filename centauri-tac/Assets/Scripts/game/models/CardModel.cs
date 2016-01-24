using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public class CardModel
    {
        public int id { get; set; } //id for instance of card, should be unique across all cards in decks/hands
        public int cardId { get; set; } //id for template card
        public int playerId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int cost { get; set; }
        public int attack { get; set; }
        public int health { get; set; }
        public int movement { get; set; }

        public List<string> tags { get; set; }

        [JsonIgnore]
        public GameObject gameObject { get; set; }
        [JsonIgnore]
        public CardView cardView { get; set; }

        public bool playable { get; set; }
    }
}

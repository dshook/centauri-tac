using Newtonsoft.Json;
using UnityEngine;

namespace ctac
{
    public class CardModel
    {
        public int id { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public int cost { get; set; }
        public int attack { get; set; }
        public int health { get; set; }

        [JsonIgnore]
        public GameObject gameObject { get; set; }

        public int playerId { get; set; }
    }
}

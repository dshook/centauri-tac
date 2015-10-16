namespace ctac
{
    public class CardModel
    {
        public int id { get; set; }
        public int playerId { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public int attack { get; set; }
        public int health { get; set; }
    }
}

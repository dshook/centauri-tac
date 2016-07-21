namespace ctac
{
    public class CardBuffModel
    {
        public int id { get; set; }
        public int cardId { get; set; }
        public string name { get; set; }
        public bool removed { get; set; }

        public int? cost { get; set; }

        public int? newCost { get; set; }
    }
}

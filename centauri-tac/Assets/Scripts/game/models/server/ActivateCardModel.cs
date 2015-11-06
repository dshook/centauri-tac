namespace ctac
{
    public class ActivateCardModel
    {
        public int id { get; set; }
        public int playerId { get; set; }
        public int cardId { get; set; }
        public PositionModel position { get; set; }
    }
}

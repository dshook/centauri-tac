namespace ctac
{
    public class PlaySpellModel
    {
        public int id { get; set; }
        public int cardTemplateId { get; set; }
        public int playerId { get; set; }
        public PositionModel position { get; set; }
    }
}

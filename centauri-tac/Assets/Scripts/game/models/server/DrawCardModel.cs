namespace ctac
{
    public class DrawCardModel : BaseAction
    {
        public int? cardId { get; set; }
        public int? cardTemplateId { get; set; }
        public int playerId { get; set; }

        public bool milled { get; set; }
        public bool overdrew { get; set; }
    }
}

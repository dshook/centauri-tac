namespace ctac
{
    public class PlaySpellModel : BaseAction
    {
        public int cardInstanceId { get; set; }
        public int cardTemplateId { get; set; }
        public int playerId { get; set; }
        public PositionModel position { get; set; }
        public int? targetPieceId { get; set; }
    }
}

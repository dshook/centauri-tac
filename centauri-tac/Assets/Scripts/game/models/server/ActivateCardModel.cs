namespace ctac
{
    public class ActivateCardModel
    {
        public int id { get; set; }
        public int playerId { get; set; }
        public int cardInstanceId { get; set; }
        public PositionModel position { get; set; }
        public PositionModel pivotPosition { get; set; }
        public int? targetPieceId { get; set; }
    }
}

namespace ctac
{
    public class SpawnPieceModel
    {
        public int id { get; set; }
        public int pieceId { get; set; }
        public int cardId { get; set; }
        public int playerId { get; set; }
        public PositionModel position { get; set; }
        public int attack { get; set; }
        public int health { get; set; }
    }
}

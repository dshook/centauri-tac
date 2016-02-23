namespace ctac
{
    public class MovePieceModel
    {
        public int id { get; set; }
        public int pieceId { get; set; }
        public PositionModel to { get; set; }
        public Direction direction { get; set; }
    }
}

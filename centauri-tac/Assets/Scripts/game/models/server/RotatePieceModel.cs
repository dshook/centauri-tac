namespace ctac
{
    public class RotatePieceModel
    {
        public RotatePieceModel(int pieceId, Direction direction)
        {
            this.pieceId = pieceId;
            this.direction = direction;
        }

        public int id { get; set; }
        public int pieceId { get; set; }
        public Direction direction { get; set; }
    }
}

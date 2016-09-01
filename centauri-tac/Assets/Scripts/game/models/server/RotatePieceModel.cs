namespace ctac
{
    public class RotatePieceModel : BaseAction
    {
        public RotatePieceModel(int pieceId, Direction direction)
        {
            this.pieceId = pieceId;
            this.direction = direction;
        }

        public int pieceId { get; set; }
        public Direction direction { get; set; }
    }
}

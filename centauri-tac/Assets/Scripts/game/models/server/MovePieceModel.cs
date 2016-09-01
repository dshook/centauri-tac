namespace ctac
{
    public class MovePieceModel : BaseAction
    {
        public int pieceId { get; set; }
        public PositionModel to { get; set; }
        public Direction direction { get; set; }
        public bool isJump { get; set; }
        public bool isTeleport { get; set; }
    }
}

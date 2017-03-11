namespace ctac
{
    public class AttackPieceModel : BaseAction
    {
        public int attackingPieceId { get; set; }
        public int targetPieceId { get; set; }
        public Direction direction { get; set; }
        public Direction targetDirection { get; set; }
    }
}

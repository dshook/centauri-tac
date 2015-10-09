namespace ctac
{
    public class AttackPieceModel
    {
        public int id { get; set; }
        public int attackingPieceId { get; set; }
        public int targetPieceId { get; set; }

        public int attackerNewHp { get; set; }
        public int targetNewHp { get; set; }
    }
}

namespace ctac
{
    public class PieceBuffModel : BaseAction
    {
        public int pieceId { get; set; }
        public string name { get; set; }
        public bool removed { get; set; }

        public int? attack { get; set; }
        public int? health { get; set; }
        public int? movement { get; set; }
        public int? range { get; set; }

        public int? newAttack { get; set; }
        public int? newHealth { get; set; }
        public int? newMovement { get; set; }
        public int? newRange { get; set; }
    }
}

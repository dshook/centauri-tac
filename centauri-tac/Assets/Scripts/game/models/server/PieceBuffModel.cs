namespace ctac
{
    public class PieceBuffModel
    {
        public int id { get; set; }
        public int pieceId { get; set; }
        public string name { get; set; }

        public int? attack { get; set; }
        public int? health { get; set; }
        public int? movement { get; set; }

        public int? newAttack { get; set; }
        public int? newHealth { get; set; }
        public int? newMovement { get; set; }
    }
}

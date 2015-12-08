namespace ctac
{
    public class PieceAttributeChangeModel
    {
        public int id { get; set; }
        public int pieceId { get; set; }

        public int? attack { get; set; }
        public int? health { get; set; }
        public int? movement { get; set; }

        public int? baseAttack { get; set; }
        public int? baseHealth { get; set; }
        public int? baseMovement { get; set; }
    }
}

namespace ctac
{
    public class PieceAuraModel : BaseAction
    {
        public int pieceId { get; set; }
        public string name { get; set; }

        public int? attack { get; set; }
        public int? health { get; set; }
        public int? movement { get; set; }
        public int? range { get; set; }
        public int? spellDamage { get; set; }
    }
}

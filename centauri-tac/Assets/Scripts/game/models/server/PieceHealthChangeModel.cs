namespace ctac
{
    public class PieceHealthChangeModel
    {
        public int id { get; set; }
        public int pieceId { get; set; }
        public int change { get; set; }
        public int? bonus { get; set; }
        public string bonusMsg { get; set; }
        public int newCurrentHealth { get; set; }
        public int newCurrentArmor { get; set; }
        public int armorChange { get; set; }
    }
}

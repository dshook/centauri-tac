namespace ctac
{
    public class PieceArmorChangeModel : BaseAction
    {
        public int pieceId { get; set; }
        public int change { get; set; }
        public int newArmor { get; set; }
    }
}

namespace ctac
{
    public class UnsummonPieceModel : BaseAction
    {
        public int pieceId { get; set; }

        //new card id for returned to hand
        public int cardId { get; set; }
    }
}

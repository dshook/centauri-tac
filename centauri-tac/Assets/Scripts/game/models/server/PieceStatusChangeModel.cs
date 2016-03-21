namespace ctac
{
    public class PieceStatusChangeModel
    {
        public int id { get; set; }
        public int pieceId { get; set; }
        public Statuses? add { get; set; }
        public Statuses? remove { get; set; }

        public Statuses statuses { get; set; }
    }
}

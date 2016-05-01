namespace ctac
{
    public class StartTargetModel
    {
        public CardModel targetingCard { get; set; }
        public Tile cardDeployPosition { get; set; }
        public ActionTarget targets { get; set; }
        public AreaTarget area { get; set; }
    }

    public class SelectTargetModel
    {
        public PieceModel piece { get; set; }
        public Tile tile { get; set; }
    }
}

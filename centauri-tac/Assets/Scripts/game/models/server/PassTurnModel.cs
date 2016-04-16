namespace ctac
{
    public class PassTurnModel
    {
        public int id { get; set; }
        public int to { get; set; }
        public int? from { get; set; }
        public int currentTurn { get; set; }

        public int toPlayerResources { get; set; }
        public int toPlayerMaxResources { get; set; }
    }
}

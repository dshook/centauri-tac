namespace ctac
{
    public class GameFinishedModel
    {
        public int id { get; set; }
        public int winnerId { get; set; }
        public string message { get; set; }
        public bool isDisconnect { get; set; }  //client only
    }
}

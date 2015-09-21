namespace ctac
{
    [Singleton]
    public class ConfigModel
    {
        public string baseUrl { get; set; }
        public string realm { get; set; }

        public string opponentUser { get; set; }
        public string opponentPw { get; set; }
    }
}


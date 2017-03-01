namespace ctac
{
    /// <summary>
    /// Players connected to the current game
    /// </summary>
    [Singleton]
    public class CurrentGameModel
    {
        public GameMetaModel game { get; set; }
    }
}

namespace ctac
{
    public class SetPlayerResourceModel
    {
        public int id { get; set; }
        public int playerId { get; set; }
        public int change { get; set; }

        public int newAmount { get; set; }
        public int newMax { get; set; }
    }
}

namespace ctac
{
    public class SetPlayerResourceModel : BaseAction
    {
        public int playerId { get; set; }
        public int change { get; set; }

        public int newAmount { get; set; }
        public int newMax { get; set; }
    }
}

namespace ctac
{
    public class PieceSpawnedModel
    {
        public SpawnPieceModel spawnPieceAction { get; set; }
        public PieceModel piece { get; set; }
        public bool alreadyDeployed { get; set; }  //For when the phantom piece is already on the board
        public bool runAsync { get; set; }
    }
}

﻿namespace ctac
{
    public class SpawnPieceModel
    {
        public int id { get; set; }
        public int pieceResourceId { get; set; }
        public int playerId { get; set; }
        public PositionModel position { get; set; }
    }
}
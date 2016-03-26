﻿namespace ctac
{
    public class PieceStatusChangeModel
    {
        public int id { get; set; }
        public int pieceId { get; set; }
        public Statuses? add { get; set; }
        public Statuses? remove { get; set; }

        public Statuses statuses { get; set; }

        public int? newAttack { get; set; }
        public int? newHealth { get; set; }
        public int? newMovement { get; set; }
    }
}
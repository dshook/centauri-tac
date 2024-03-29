﻿using System.Collections.Generic;

namespace ctac
{
    public class SpawnPieceModel : BaseAction
    {
        public int? pieceId { get; set; }
        public int? cardInstanceId { get; set; }
        public int cardTemplateId { get; set; }
        public int playerId { get; set; }
        public PositionModel position { get; set; }
        public Direction direction { get; set; }

        public List<string> tags { get; set; }
    }
}

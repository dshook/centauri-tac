﻿using System.Collections.Generic;

namespace ctac
{
    public class MovePathFoundModel
    {
        public PieceModel piece { get; set; }
        public Tile startTile { get; set; }
        public Tile endTile { get; set; }
        public bool isAttack { get; set; }
        public List<Tile> tiles { get; set; }
    }
}

using UnityEngine;

namespace ctac
{
    [Singleton]
    public class RaycastModel
    {
        /// <summary>
        /// The tile the mouse is over, if any
        /// </summary>
        public Tile tile { get; set; }

        /// <summary>
        /// The piece the mouse is over, if any
        /// </summary>
        public PieceView piece { get; set; }

        public RaycastHit? worldHit { get; set; }
        public RaycastHit? cardCanvasHit { get; set; }
    }
}

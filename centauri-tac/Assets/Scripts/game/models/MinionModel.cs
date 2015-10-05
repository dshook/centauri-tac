
using UnityEngine;

namespace ctac
{
    public class MinionModel
    {
        public int id { get; set; }

        public int playerId { get; set; }

        public GameObject gameObject { get; set; }

        public bool currentPlayerHasControl { get; set; }

        public bool isMoving { get; set; }

        public bool hasMoved { get; set; }

        public bool hasAttacked { get; set; }

        public Vector2 tilePosition {
            get
            {
                return gameObject.transform.position.ToTileCoordinates();
            }
        }

        public int attack { get; set; }
        public int health { get; set; }
    }
}

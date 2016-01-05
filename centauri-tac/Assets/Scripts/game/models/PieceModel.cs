using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public class PieceModel
    {
        public int id { get; set; }

        public int playerId { get; set; }

        public int cardId { get; set; }

        public string name { get; set; }

        public GameObject gameObject { get; set; }

        public bool currentPlayerHasControl { get; set; }

        public bool isMoving { get; set; }

        public bool hasMoved { get; set; }

        public bool hasAttacked { get; set; }

        public List<string> tags { get; set; }

        public Vector2 tilePosition {
            get
            {
                return gameObject.transform.position.ToTileCoordinates();
            }
        }

        public int attack { get; set; }
        public int health { get; set; }
        public int movement { get; set; }

        public int baseAttack { get; set; }
        public int baseHealth { get; set; }
        public int baseMovement { get; set; }
    }
}

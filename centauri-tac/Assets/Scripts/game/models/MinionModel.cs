﻿
using UnityEngine;

namespace ctac
{
    public class MinionModel
    {
        public GameObject gameObject { get; set; }

        public bool isMoving { get; set; }

        public bool hasMoved { get; set; }

        public bool hasAttacked { get; set; }

        public Vector2 tilePosition {
            get
            {
                return gameObject.transform.position.ToTileCoordinates();
            }
        }
    }
}
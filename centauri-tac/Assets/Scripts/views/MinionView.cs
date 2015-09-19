﻿using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;

namespace ctac {
    public class MinionView : View
    {
        public MinionModel minion { get; set; }

        private List<Tile> path;
        private Vector3? destination = null;

        private float moveSpeed = 3f;

        void Update()
        {
            if (path != null && path.Count > 0)
            {
                if (destination == null)
                {
                    destination = path[0].gameObject.transform.position;
                    path.RemoveAt(0);
                }
            }

            if (destination != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination.Value, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, destination.Value) < 0.01)
                {
                    transform.position = destination.Value;
                    destination = null;
                }
            }

        }

        public void MovePath(List<Tile> path)
        {
            this.path = path;
        }
    }
}

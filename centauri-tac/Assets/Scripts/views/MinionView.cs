using strange.extensions.mediation.impl;
using UnityEngine;

namespace ctac {
    public class MinionView : View
    {
        public IMinionModel minion { get; set; }

        private Vector3? destination = null;

        private float moveSpeed = 3f;

        void Update()
        {
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

        public void Move(Tile dest)
        {
            destination = dest.gameObject.transform.position;
        }
    }
}

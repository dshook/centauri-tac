using strange.extensions.mediation.impl;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ctac {
    public class MinionView : View
    {
        public MinionModel minion { get; set; }

        private List<Tile> path;
        private Vector3? destination = null;

        private float moveSpeed = 3f;

        private TextMeshPro attackText;
        private TextMeshPro healthText;

        protected override void Start()
        {
            attackText = minion.gameObject.transform.FindChild("Attack").GetComponent<TextMeshPro>();
            healthText = minion.gameObject.transform.FindChild("Health").GetComponent<TextMeshPro>();
        }

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
                minion.isMoving = true;
                transform.position = Vector3.MoveTowards(transform.position, destination.Value, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, destination.Value) < 0.01)
                {
                    transform.position = destination.Value;
                    destination = null;
                    minion.isMoving = false;
                }
            }

            if (attackText != null && healthText != null)
            {
                attackText.text = minion.attack.ToString();
                healthText.text = minion.health.ToString();
            }

        }

        public void MovePath(List<Tile> path)
        {
            this.path = path;
        }
    }
}

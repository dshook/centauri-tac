using strange.extensions.mediation.impl;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ctac {
    public class MinionView : View
    {
        public MinionModel minion { get; set; }

        private List<Tile> path = new List<Tile>();
        private Vector3? destination = null;

        private float moveSpeed = 1f;

        private TextMeshPro attackText;
        private TextMeshPro healthText;

        private SpriteRenderer spriteRenderer;
        private Material spriteDefault;
        private Material moveOutline;

        protected override void Start()
        {
            attackText = minion.gameObject.transform.FindChild("Attack").GetComponent<TextMeshPro>();
            healthText = minion.gameObject.transform.FindChild("Health").GetComponent<TextMeshPro>();

            spriteRenderer = minion.gameObject.GetComponentInChildren<SpriteRenderer>();
            spriteDefault = Resources.Load("Materials/SpriteDefault") as Material;
            moveOutline = Resources.Load("Materials/MoveOutlineMat") as Material;
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
                iTweenExtensions.MoveTo(minion.gameObject, destination.Value, moveSpeed, 0, EaseType.linear);
                if (Vector3.Distance(transform.position, destination.Value) < 0.01)
                {
                    transform.position = destination.Value;
                    destination = null;
                    minion.isMoving = false;
                }
            }

            UpdateText(attackText, minion.attack, minion.originalAttack);
            UpdateText(healthText, minion.health, minion.originalHealth);

            if (minion.currentPlayerHasControl && !minion.hasMoved)
            {
                spriteRenderer.material = moveOutline;
            }
            else
            {
                spriteRenderer.material = spriteDefault;
            }

        }

        private void UpdateText(TextMeshPro text, int current, int original)
        {
            if(text == null) return;

            text.text = current.ToString();
            if (current > original)
            {
                text.color = Color.green;
            }
            else if (current < original)
            {
                text.color = Color.red;
            }
            else
            {
                text.color = Color.white;
            }
        }

        public void AddToPath(Tile tile)
        {
            this.path.Add(tile);
        }
    }
}

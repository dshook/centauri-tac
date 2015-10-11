using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ctac {
    public class MinionView : View
    {
        public MinionModel minion { get; set; }

        public GameObject attackGO;
        public GameObject healthGO;
        public TextMeshPro attackText;
        public TextMeshPro healthText;

        private SpriteRenderer spriteRenderer;
        private Material spriteDefault;
        private Material moveOutline;

        protected override void Start()
        {
            attackGO = minion.gameObject.transform.FindChild("Attack").gameObject;
            healthGO = minion.gameObject.transform.FindChild("Health").gameObject;
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();

            spriteRenderer = minion.gameObject.GetComponentInChildren<SpriteRenderer>();
            spriteDefault = Resources.Load("Materials/SpriteDefault") as Material;
            moveOutline = Resources.Load("Materials/MoveOutlineMat") as Material;

            attackText.text = minion.attack.ToString();
            healthText.text = minion.health.ToString();
        }

        void Update()
        {
            if (minion.currentPlayerHasControl && !minion.hasMoved)
            {
                spriteRenderer.material = moveOutline;
            }
            else
            {
                spriteRenderer.material = spriteDefault;
            }

        }

        public class MoveAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }

            public GameObject minion { get; set; }
            public Vector3 destination { get; set; }
            private float moveSpeed = 0.3f;

            public void Update()
            {
                iTweenExtensions.MoveTo(minion.gameObject, destination, moveSpeed, 0, EaseType.linear);
                if (Vector3.Distance(minion.transform.position, destination) < 0.01)
                {
                    minion.transform.position = destination;
                    Complete = true;
                }
            }
        }

        public class UpdateTextAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }

            public MinionAttackedAnimationSignal attackFinished { get; set; }
            public MinionModel minion { get; set; }
            public GameObject textGO { get; set; }
            public TextMeshPro text { get; set; }
            public int current { get; set; }
            public int original { get; set; }
            private Vector3 punchSize = new Vector3(1.5f, 1.5f, 1.5f);

            public void Update()
            {
                if(text == null) return;

                text.text = current.ToString();
                iTweenExtensions.PunchScale(textGO, punchSize, 1.5f, 0);
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
                Complete = true;
                attackFinished.Dispatch(minion);
            }
        }

        public class DieAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }

            public PieceDiedSignal pieceDied { get; set; }
            public MinionModel minion { get; set; }

            public void Update()
            {
                iTweenExtensions.ScaleTo(minion.gameObject, Vector3.zero, 1.5f, 0, EaseType.easeInQuart);
                if (minion.gameObject.transform.localScale.x < 0.01f)
                {
                    minion.gameObject.transform.localScale = Vector3.zero;
                    Complete = true;
                    pieceDied.Dispatch(minion);
                }
            }
        }
    }
}

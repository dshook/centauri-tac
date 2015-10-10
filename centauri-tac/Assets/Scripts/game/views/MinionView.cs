using strange.extensions.mediation.impl;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ctac {
    public class MinionView : View
    {
        public MinionModel minion { get; set; }

        private List<IAnimate> animations = new List<IAnimate>();

        public TextMeshPro attackText;
        public TextMeshPro healthText;

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
            if (animations.Count > 0)
            {
                var anim = animations[0];
                anim.Update();
                if (anim.Complete)
                {
                    animations.RemoveAt(0);
                }
            }

            if (minion.currentPlayerHasControl && !minion.hasMoved)
            {
                spriteRenderer.material = moveOutline;
            }
            else
            {
                spriteRenderer.material = spriteDefault;
            }

        }

        public void AddAnim(IAnimate anim)
        {
            this.animations.Add(anim);
        }

        public interface IAnimate
        {
            void Update();
            bool Complete { get; }
        }

        public class MoveAnim : IAnimate
        {
            public bool Complete { get; set; }

            public GameObject minion { get; set; }
            public Vector3 destination { get; set; }
            private float moveSpeed = 0.4f;

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

            public TextMeshPro text { get; set; }
            public int current { get; set; }
            public int original { get; set; }

            public void Update()
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
                Complete = true;
            }
        }

        public class DieAnim : IAnimate
        {
            public bool Complete { get; set; }

            public GameObject minion { get; set; }

            public void Update()
            {
                iTweenExtensions.ScaleTo(minion, Vector3.zero, 1.5f, 0, EaseType.easeInOutBounce);
                if (minion.transform.localScale.x < 0.01f)
                {
                    minion.transform.localScale = Vector3.zero;
                    Complete = true;
                }
            }
        }
    }
}

﻿using ctac.signals;
using strange.extensions.mediation.impl;
using TMPro;
using UnityEngine;

namespace ctac {
    public class PieceView : View
    {
        public PieceModel piece { get; set; }

        public GameObject attackGO;
        public GameObject healthGO;
        public TextMeshPro attackText;
        public TextMeshPro healthText;

        private SpriteRenderer spriteRenderer;
        private Material spriteDefault;
        private Material moveOutline;

        protected override void Start()
        {
            attackGO = piece.gameObject.transform.FindChild("Attack").gameObject;
            healthGO = piece.gameObject.transform.FindChild("Health").gameObject;
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();

            spriteRenderer = piece.gameObject.GetComponentInChildren<SpriteRenderer>();
            spriteDefault = Resources.Load("Materials/SpriteDefault") as Material;
            moveOutline = Resources.Load("Materials/MoveOutlineMat") as Material;

            attackText.text = piece.attack.ToString();
            healthText.text = piece.health.ToString();
        }

        void Update()
        {
            if(piece == null) return;

            if (piece.currentPlayerHasControl && !piece.hasMoved)
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
            public float? postDelay { get { return null; } }

            public PieceModel piece { get; set; }
            public PieceFinishedMovingSignal finishedMoving { get; set; }
            public Vector3 destination { get; set; }
            private float moveSpeed = 0.3f;

            public void Update()
            {
                iTweenExtensions.MoveTo(piece.gameObject, destination, moveSpeed, 0, EaseType.linear);
                if (Vector3.Distance(piece.gameObject.transform.position, destination) < 0.01)
                {
                    piece.gameObject.transform.position = destination;
                    Complete = true;
                    finishedMoving.Dispatch(piece);
                }
            }
        }

        public class UpdateTextAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceAttackedAnimationSignal attackFinished { get; set; }
            public PieceModel piece { get; set; }
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
                attackFinished.Dispatch(piece);
            }
        }

        public class DieAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceDiedSignal pieceDied { get; set; }
            public PieceModel piece { get; set; }

            public void Update()
            {
                iTweenExtensions.ScaleTo(piece.gameObject, Vector3.zero, 1.5f, 0, EaseType.easeInQuart);
                if (piece.gameObject.transform.localScale.x < 0.01f)
                {
                    piece.gameObject.transform.localScale = Vector3.zero;
                    Complete = true;
                    pieceDied.Dispatch(piece);
                }
            }
        }
    }
}

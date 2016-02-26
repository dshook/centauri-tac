﻿using ctac.signals;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public GameObject damageSplat;
        public TextMeshPro damageSplatText;
        public TextMeshPro damageSplatBonusText;

        public GameObject eventIconContainer;
        public GameObject circleBg;
        public GameObject deathIcon;
        public GameObject eventIcon;
        public GameObject model;
        //list of event tags to show icon on minion for 
        private static List<string> eventTags = new List<string>() {
            "damaged", "attacks", "cardDrawn", "turnEnd", "turnStart", "playSpell"
        };
        public bool targetCandidate = false;

        //private SpriteRenderer spriteRenderer;
        //private Material spriteDefault;
        private Material moveOutline;
        private Material attackOutline;
        private Material targetOutline;

        private int currentTurnPlayerId;

        protected override void Start()
        {
            model = piece.gameObject.transform.FindChild("Model").gameObject;
            attackGO = piece.gameObject.transform.FindChild("Attack").gameObject;
            healthGO = piece.gameObject.transform.FindChild("Health").gameObject;
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            damageSplat = piece.gameObject.transform.FindChild("DamageSplat").gameObject;
            damageSplatText = damageSplat.transform.FindChild("Text").GetComponent<TextMeshPro>();
            damageSplatBonusText = damageSplat.transform.FindChild("Bonus").GetComponent<TextMeshPro>();

            eventIconContainer = piece.gameObject.transform.FindChild("EventIconContainer").gameObject;
            circleBg = eventIconContainer.transform.FindChild("CircleBg").gameObject;
            eventIcon = eventIconContainer.transform.FindChild("Event").gameObject;
            deathIcon = eventIconContainer.transform.FindChild("Death").gameObject;

            //spriteRenderer = piece.gameObject.GetComponentInChildren<SpriteRenderer>();
            //spriteDefault = Resources.Load("Materials/SpriteDefault") as Material;
            moveOutline = Resources.Load("Materials/MoveOutlineMat") as Material;
            attackOutline = Resources.Load("Materials/AttackOutlineMat") as Material;
            targetOutline = Resources.Load("Materials/TargetOutlineMat") as Material;

            attackText.text = piece.attack.ToString();
            healthText.text = piece.health.ToString();

            //set icon visibility based on tags
            if (piece.tags.Contains("death"))
            {
                circleBg.SetActive(true);
                deathIcon.SetActive(true);
            }
            var eventTagCount = piece.tags.Join(eventTags, p => p, e => e, (p, e) => p).Count();
            if (eventTagCount > 0)
            {
                circleBg.SetActive(true);
                eventIcon.SetActive(true);
            }
            //rotate to model direction
            model.gameObject.transform.rotation = Quaternion.Euler(DirectionAngle.angle[piece.direction]);
        }

        void Update()
        {
            if(piece == null) return;

            if (targetCandidate)
            {
                //spriteRenderer.material = targetOutline;
            }
            else if (currentTurnPlayerId == piece.playerId && piece.currentPlayerHasControl && !piece.hasMoved)
            {
                //spriteRenderer.material = moveOutline;
            }
            else if (currentTurnPlayerId == piece.playerId && piece.currentPlayerHasControl && !piece.hasAttacked && piece.attack > 0) {
                //spriteRenderer.material = attackOutline;
            }
            else
            {
                //spriteRenderer.material = spriteDefault;
            }
        }

        public void UpdateTurn(int newPlayerId)
        {
            currentTurnPlayerId = newPlayerId;
        }

        public class RotateAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public Vector3 destAngle { get; set; }
            private float rotateSpeed = 0.3f;

            public void Update()
            {
                iTweenExtensions.RotateTo(piece.model.gameObject, destAngle, rotateSpeed, 0f);

                if (Vector3.Distance(piece.model.gameObject.transform.rotation.eulerAngles, destAngle) < 0.01)
                {
                    piece.model.gameObject.transform.rotation = Quaternion.Euler(destAngle);
                    Complete = true;
                }
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

        public class TakeDamageAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public GameObject damageSplat { get; set; }
            public TextMeshPro text { get; set; }
            public TextMeshPro bonusText { get; set; }
            public int damageTaken { get; set; }
            public int? bonus { get; set; }
            public string bonusMsg { get; set; }

            public void Update()
            {
                text.text = Math.Abs(damageTaken).ToString();
                iTweenExtensions.ScaleTo(damageSplat, Vector3.one, 0.5f, 0);
                iTweenExtensions.ScaleTo(damageSplat, Vector3.zero, 0.8f, 1);
                if (bonus.HasValue)
                {
                    bonusText.text = Math.Abs(bonus.Value).ToString() + " " + bonusMsg;
                }
                else
                {
                    bonusText.text = "";
                }
                Complete = true;
            }
        }

        public class UpdateTextAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceTextAnimationFinishedSignal animFinished { get; set; }
            public PieceModel piece { get; set; }
            public GameObject textGO { get; set; }
            public TextMeshPro text { get; set; }
            public int current { get; set; }
            public int original { get; set; }
            public int change { get; set; }
            private Vector3 punchSize = new Vector3(1.5f, 1.5f, 1.5f);

            public void Update()
            {
                if(text == null) return;

                text.text = current.ToString();
                if (change != 0)
                {
                    iTweenExtensions.PunchScale(textGO, punchSize, 1.5f, 0);
                }
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
                animFinished.Dispatch(piece);
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

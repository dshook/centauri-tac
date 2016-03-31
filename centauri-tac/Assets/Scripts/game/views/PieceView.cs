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
        public GameObject shield;
        public GameObject cloak;
        public GameObject paralyze;
        public GameObject root;

        public GameObject faceCameraContainer;
        public GameObject eventIconContainer;
        public GameObject circleBg;
        public GameObject deathIcon;
        public GameObject eventIcon;
        public GameObject model;
        private GameObject textContainer;
        //list of event tags to show icon on minion for 
        private static List<string> eventTags = new List<string>() {
            "damaged", "attacks", "cardDrawn", "turnEnd", "turnStart", "playSpell"
        };
        public bool targetCandidate = false;

        private MeshRenderer meshRenderer;
        private float outlineWidth = 3f;

        private int currentTurnPlayerId;

        protected override void Start()
        {
            model = piece.gameObject.transform.FindChild("Model").gameObject;
            faceCameraContainer = piece.gameObject.transform.FindChild("FaceCameraContainer").gameObject;
            textContainer = faceCameraContainer.transform.FindChild("TextContainer").gameObject;
            attackGO = textContainer.transform.FindChild("Attack").gameObject;
            healthGO = textContainer.transform.FindChild("Health").gameObject;
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            damageSplat = faceCameraContainer.transform.FindChild("DamageSplat").gameObject;
            damageSplatText = damageSplat.transform.FindChild("Text").GetComponent<TextMeshPro>();
            damageSplatBonusText = damageSplat.transform.FindChild("Bonus").GetComponent<TextMeshPro>();
            shield = faceCameraContainer.transform.FindChild("Shield").gameObject;
            cloak = faceCameraContainer.transform.FindChild("Cloak").gameObject;
            paralyze = faceCameraContainer.transform.FindChild("Paralyze").gameObject;
            root = faceCameraContainer.transform.FindChild("Root").gameObject;

            eventIconContainer = faceCameraContainer.transform.FindChild("EventIconContainer").gameObject;
            circleBg = eventIconContainer.transform.FindChild("CircleBg").gameObject;
            eventIcon = eventIconContainer.transform.FindChild("Event").gameObject;
            deathIcon = eventIconContainer.transform.FindChild("Death").gameObject;

            meshRenderer = model.GetComponentInChildren<MeshRenderer>();

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

            faceCameraContainer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

            if (targetCandidate)
            {
                meshRenderer.material.SetColor("_OutlineColor", Color.magenta);
                meshRenderer.material.SetFloat("_Outline", outlineWidth);
            }
            else if (
                currentTurnPlayerId == piece.playerId 
                && piece.currentPlayerHasControl && !piece.hasMoved
                && !FlagsHelper.IsSet(piece.statuses, Statuses.Paralyze)
                && !FlagsHelper.IsSet(piece.statuses, Statuses.Root)
            )
            {
                meshRenderer.material.SetColor("_OutlineColor", Color.green);
                meshRenderer.material.SetFloat("_Outline", outlineWidth);
            }
            else if (
                currentTurnPlayerId == piece.playerId 
                && piece.currentPlayerHasControl 
                && !piece.hasAttacked && piece.attack > 0
                && !FlagsHelper.IsSet(piece.statuses, Statuses.Paralyze)
            ) {
                meshRenderer.material.SetColor("_OutlineColor", Color.cyan);
                meshRenderer.material.SetFloat("_Outline", outlineWidth);
            }
            else
            {
                meshRenderer.material.SetColor("_OutlineColor", Color.black);
                meshRenderer.material.SetFloat("_Outline", 0.5f);
            }

            //statuses
            if (FlagsHelper.IsSet(piece.statuses, Statuses.Shield))
            {
                shield.transform.localScale = Vector3.one;
            }
            else
            {
                shield.transform.localScale = Vector3.zero;
            }

            if (FlagsHelper.IsSet(piece.statuses, Statuses.Paralyze))
            {
                paralyze.transform.localScale = Vector3.one;
            }
            else
            {
                paralyze.transform.localScale = Vector3.zero;
            }

            if (FlagsHelper.IsSet(piece.statuses, Statuses.Cloak))
            {
                cloak.transform.localScale = Vector3.one;
            }
            else
            {
                cloak.transform.localScale = Vector3.zero;
            }

            if (FlagsHelper.IsSet(piece.statuses, Statuses.Root))
            {
                root.transform.localScale = Vector3.one;
            }
            else
            {
                root.transform.localScale = Vector3.zero;
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
                if (bonus.HasValue && bonus != 0)
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

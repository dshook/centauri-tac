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
        public GameObject armorGO;
        public TextMeshPro attackText;
        public TextMeshPro healthText;
        public TextMeshPro armorText;
        public GameObject damageSplat;
        public TextMeshPro damageSplatText;
        public TextMeshPro damageSplatBonusText;
        public GameObject shield;
        public GameObject cloak;
        public GameObject paralyze;
        public GameObject root;
        public GameObject armorBG;

        public GameObject hpBarContainer;
        public GameObject hpBar;
        public GameObject hpBarfill;

        public GameObject faceCameraContainer;
        public GameObject eventIconContainer;
        public GameObject circleBg;
        public GameObject deathIcon;
        public GameObject eventIcon;
        public GameObject model;
        private GameObject textContainer;
        public bool targetCandidate = false;
        public bool enemiesInRange = false;

        private MeshRenderer meshRenderer;
        private float outlineWidth = 3f;

        public int currentTurnPlayerId;

        protected override void Start()
        {
            model = piece.gameObject.transform.FindChild("Model").gameObject;
            faceCameraContainer = piece.gameObject.transform.FindChild("FaceCameraContainer").gameObject;
            textContainer = faceCameraContainer.transform.FindChild("TextContainer").gameObject;
            attackGO = textContainer.transform.FindChild("Attack").gameObject;
            healthGO = textContainer.transform.FindChild("Health").gameObject;
            armorGO = textContainer.transform.FindChild("Armor").gameObject;
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            armorText = armorGO.GetComponent<TextMeshPro>();
            damageSplat = faceCameraContainer.transform.FindChild("DamageSplat").gameObject;
            damageSplatText = damageSplat.transform.FindChild("Text").GetComponent<TextMeshPro>();
            damageSplatBonusText = damageSplat.transform.FindChild("Bonus").GetComponent<TextMeshPro>();
            shield = faceCameraContainer.transform.FindChild("Shield").gameObject;
            cloak = faceCameraContainer.transform.FindChild("Cloak").gameObject;
            paralyze = faceCameraContainer.transform.FindChild("Paralyze").gameObject;
            root = faceCameraContainer.transform.FindChild("Root").gameObject;
            armorBG = faceCameraContainer.transform.FindChild("Armor").gameObject;

            hpBarContainer = faceCameraContainer.transform.FindChild("HpBarContainer").gameObject;
            hpBar = hpBarContainer.transform.FindChild("hpbar").gameObject;
            hpBarfill = hpBarContainer.transform.FindChild("hpbarfill").gameObject;

            eventIconContainer = faceCameraContainer.transform.FindChild("EventIconContainer").gameObject;
            circleBg = eventIconContainer.transform.FindChild("CircleBg").gameObject;
            eventIcon = eventIconContainer.transform.FindChild("Event").gameObject;
            deathIcon = eventIconContainer.transform.FindChild("Death").gameObject;

            meshRenderer = model.GetComponentInChildren<MeshRenderer>();

            attackText.text = piece.attack.ToString();
            healthText.text = piece.health.ToString();

            circleBg.SetActive(false);
            deathIcon.SetActive(false);
            eventIcon.SetActive(false);

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
                && piece.movement > 0
            )
            {
                meshRenderer.material.SetColor("_OutlineColor", Color.green);
                meshRenderer.material.SetFloat("_Outline", outlineWidth);
            }
            else if (
                currentTurnPlayerId == piece.playerId 
                && piece.currentPlayerHasControl 
                && piece.attackCount < piece.maxAttacks 
                && piece.attack > 0
                && !FlagsHelper.IsSet(piece.statuses, Statuses.Paralyze)
                && !FlagsHelper.IsSet(piece.statuses, Statuses.CantAttack)
                && enemiesInRange
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

            circleBg.SetActive(false);
            deathIcon.SetActive(false);
            eventIcon.SetActive(false);
            //event icons
            if (piece.hasDeathEvent)
            {
                circleBg.SetActive(true);
                deathIcon.SetActive(true);
            }
            else if (piece.hasEvent)
            {
                circleBg.SetActive(true);
                eventIcon.SetActive(true);
            }
        }


        private void InitHpBar()
        {

        }

        public void UpdateTurn(int newPlayerId)
        {
            currentTurnPlayerId = newPlayerId;
        }

        public class SpawnAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public MapModel map { get; set; }
            public IMapService mapService { get; set; }

            private Vector3 destPosition { get; set; }
            private Vector3 startOffset = new Vector3(0, 5f, 0);
            private float dropTime = 0.7f;
            private float pieceMagnitude = 0f;
            private float pieceDuration = 0f;

            //private Tile originTile { get; set; }
            private List<Tile> oneRing { get; set; }
            private List<Tile> twoRing { get; set; }

            public void Init()
            {
                destPosition = piece.gameObject.transform.position;
                piece.gameObject.transform.position = destPosition + startOffset;

                pieceMagnitude = Math.Min(1f, (piece.piece.health + piece.piece.attack) / 20f);
                pieceDuration = Math.Min(1f, (piece.piece.health + piece.piece.attack) / 10f);

                //originTile = map.tiles[piece.piece.tilePosition];
                oneRing = mapService.GetKingTilesInRadius(piece.piece.tilePosition, 1).Values.ToList();
                twoRing = mapService.GetKingTilesInRadius(piece.piece.tilePosition, 2).Values.Except(oneRing).ToList();
            }
            public void Update()
            {
                iTweenExtensions.MoveTo(piece.gameObject, destPosition, dropTime, 0, EaseType.easeInQuart);

                if (Vector3.Distance(piece.gameObject.transform.position, destPosition) < 0.01)
                {
                    piece.gameObject.transform.position = destPosition;

                    foreach (var tile in oneRing)
                    {
                        var tileBounce = tile.gameObject.AddComponent<TileBounce>();
                        tileBounce.magnitudeMult = pieceMagnitude;
                        tileBounce.waveDuration *= pieceDuration;
                    }
                    foreach (var tile in twoRing)
                    {
                        var tileBounce = tile.gameObject.AddComponent<TileBounce>();
                        tileBounce.magnitudeMult = pieceMagnitude * .5f;
                        tileBounce.delay = .2f;
                        tileBounce.waveDuration = (tileBounce.waveDuration * pieceDuration) + .2f;
                    }

                    Complete = true;
                }
            }
        }

        public class RotateAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public Vector3 destAngle { get; set; }
            private float rotateSpeed = 0.3f;

            public void Init() { }
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
            public bool isTeleport { get; set; }

            private float moveTime = 0.25f;
            Vector3 curveHeight = new Vector3(0, 0.30f, 0);
            private float curveMult = 1.0f;
            private BezierSpline moveSpline;
            private SplineWalker walker;
            private bool firstRun = true;

            public void Init()
            {
                moveSpline = GameObject.Find("PieceMoveSpline").GetComponent<BezierSpline>();
            }
            public void Update()
            {
                if (firstRun)
                {
                    firstRun = false;

                    var start = piece.gameObject.transform.position;
                    var diffVector = destination - start;

                    if (isTeleport)
                    {
                        curveMult = 100f;
                    }

                    var secondControl = (diffVector * 0.2f) + (curveHeight * diffVector.magnitude * curveMult) + start;
                    var thirdControl = (diffVector * 0.8f) + (curveHeight * diffVector.magnitude * curveMult) + start;

                    moveSpline.SetControlPoint(0, start);
                    moveSpline.SetControlPoint(1, secondControl);
                    moveSpline.SetControlPoint(2, thirdControl);
                    moveSpline.SetControlPoint(3, destination);

                    walker = piece.gameObject.AddComponent<SplineWalker>();
                    walker.spline = moveSpline;
                    walker.duration = moveTime;
                    walker.lookForward = false;
                    walker.mode = SplineWalkerMode.Once;
                }

                //if (Vector3.Distance(piece.gameObject.transform.position, destination) < 0.01)
                if(walker.progress > 0.99)
                {
                    piece.gameObject.transform.position = destination;
                    Complete = true;
                    Destroy(walker);
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

            public void Init() { }
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

            public void Init() { }
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

        public class UpdateArmorAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceModel piece { get; set; }
            public GameObject textGO { get; set; }
            public GameObject textBG { get; set; }
            public TextMeshPro text { get; set; }
            public int current { get; set; }
            public int change { get; set; }
            private Vector3 punchSize = new Vector3(1.5f, 1.5f, 1.5f);

            public void Init() { }
            public void Update()
            {
                if(text == null) return;

                text.text = current.ToString();

                //enable stuff if just getting armor for the first time
                if (!textGO.activeSelf && current != 0)
                {
                    textGO.SetActive(true);
                    iTweenExtensions.ScaleTo(textBG, Vector3.one, 0.5f, 0, EaseType.easeOutElastic);
                }

                if (change != 0)
                {
                    iTweenExtensions.PunchScale(textGO, punchSize, 1.5f, 0);
                }

                //disable stuff if no more armor
                if (textGO.activeSelf && current == 0)
                {
                    textGO.SetActive(false);
                    iTweenExtensions.ScaleTo(textBG, Vector3.zero, 0.5f, 0, EaseType.easeInCubic);
                }

                Complete = true;
            }
        }

        public class DieAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceDiedSignal pieceDied { get; set; }
            public PieceModel piece { get; set; }

            public void Init() { }
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

        public class UnsummonAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public PieceDiedSignal pieceDied { get; set; }

            private Vector3 destPosition { get; set; }
            private Vector3 endOffset = new Vector3(0, 5f, 0);
            private float dropTime = 0.7f;

            public void Init()
            {
                destPosition = piece.gameObject.transform.position + endOffset;
            }
            public void Update()
            {
                iTweenExtensions.MoveTo(piece.gameObject, destPosition, dropTime, 0, EaseType.easeOutQuart);

                if (Vector3.Distance(piece.gameObject.transform.position, destPosition) < 0.01)
                {
                    piece.gameObject.transform.position = destPosition;

                    Complete = true;
                    pieceDied.Dispatch(piece.piece);
                }
            }
        }
    }
}

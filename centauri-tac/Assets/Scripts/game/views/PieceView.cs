﻿using ctac.signals;
using strange.extensions.mediation.impl;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ctac.util;
using HighlightingSystem;

namespace ctac {
    public class PieceView : View
    {
        [Inject]
        public IResourceLoaderService loader { get; set; }

        public PieceModel piece { get; set; }

        public GameObject attackGO;
        public GameObject healthGO;
        public GameObject armorGO;
        public TextMeshPro attackText;
        public TextMeshPro healthText;
        public TextMeshPro armorText;
        public GameObject shield;
        public GameObject cloak;
        public GameObject paralyze;
        public GameObject root;
        public GameObject armorBG;

        public GameObject hpBarContainer;
        public GameObject hpBar;
        public GameObject hpBarfill;
        public SVGRenderer hpBarRenderer;
        public SVGRenderer hpBarOutlineRenderer;
        public SpriteRenderer hpBarFillRenderer;
        private Color32 hpBarFillFriendlyColor = Colors.friendlyColor;
        private Color32 hpBarFillEnemyColor = Colors.enemyColor;
        private GameObject canAttackIndicator;
        private GameObject canMoveIndicator;
        private SVGRenderer canAttackRenderer;
        private SVGRenderer canMoveRenderer;

        public GameObject faceCameraContainer;
        public GameObject eventIconContainer;
        public GameObject deathIcon;
        public GameObject eventIcon;
        public GameObject rangeIcon;
        public GameObject auraIcon;
        public GameObject tauntIcon;
        public GameObject cantAttackIcon;
        public GameObject dyadStrikeIcon;
        public GameObject silenceIcon;
        public GameObject flyingIcon;
        public GameObject cleaveIcon;
        public GameObject piercingIcon;

        public GameObject model;
        private GameObject textContainer;
        public bool targetCandidate = false;
        public bool enemiesInRange = false;

        public Material meshMaterial;
        private Highlighter highlight;
        private Dictionary<Statuses, GameObject> statusIcons;

        private bool prevHpBarPulsing = false;
        private Color prevHpBarColor = Colors.invisible;

        public Animator anim;

        protected override void Start()
        {
            model = piece.gameObject.transform.Find("Model").gameObject;
            faceCameraContainer = piece.gameObject.transform.Find("FaceCameraContainer").gameObject;
            anim = piece.gameObject.GetComponentInChildren<Animator>();

            hpBarContainer = faceCameraContainer.transform.Find("HpBarContainer").gameObject;
            hpBar = hpBarContainer.transform.Find("hpbar").gameObject;
            hpBarfill = hpBarContainer.transform.Find("HpBarFill").gameObject;
            hpBarFillRenderer = hpBarfill.GetComponent<SpriteRenderer>();
            hpBarRenderer = hpBar.GetComponent<SVGRenderer>();
            hpBarOutlineRenderer = hpBarContainer.transform.Find("hpbar outline").gameObject.GetComponent<SVGRenderer>();
            canAttackIndicator = hpBarContainer.transform.Find("canAttack").gameObject;
            canMoveIndicator = hpBarContainer.transform.Find("canMove").gameObject;
            canAttackRenderer = canAttackIndicator.GetComponent<SVGRenderer>();
            canMoveRenderer = canMoveIndicator.GetComponent<SVGRenderer>();

            textContainer = hpBarContainer.transform.Find("TextContainer").gameObject;
            attackGO = textContainer.transform.Find("Attack").gameObject;
            healthGO = textContainer.transform.Find("Health").gameObject;
            armorGO = textContainer.transform.Find("Armor").gameObject;
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            armorText = armorGO.GetComponent<TextMeshPro>();
            shield = faceCameraContainer.transform.Find("Shield").gameObject;
            cloak = faceCameraContainer.transform.Find("Cloak").gameObject;
            paralyze = faceCameraContainer.transform.Find("Paralyze").gameObject;
            root = faceCameraContainer.transform.Find("Root").gameObject;
            armorBG = faceCameraContainer.transform.Find("Armor").gameObject;

            eventIconContainer = faceCameraContainer.transform.Find("EventIconContainer").gameObject;
            eventIcon = eventIconContainer.transform.Find("Event").gameObject;
            deathIcon = eventIconContainer.transform.Find("Death").gameObject;
            rangeIcon = eventIconContainer.transform.Find("Range").gameObject;
            auraIcon = eventIconContainer.transform.Find("Aura").gameObject;
            tauntIcon = eventIconContainer.transform.Find("Taunt").gameObject;
            cantAttackIcon = eventIconContainer.transform.Find("CantAttack").gameObject;
            dyadStrikeIcon = eventIconContainer.transform.Find("DyadStrike").gameObject;
            silenceIcon = eventIconContainer.transform.Find("Silence").gameObject;
            flyingIcon = eventIconContainer.transform.Find("Flying").gameObject;
            cleaveIcon = eventIconContainer.transform.Find("Cleave").gameObject;
            piercingIcon = eventIconContainer.transform.Find("Piercing").gameObject;

            statusIcons = new Dictionary<Statuses, GameObject>()
            {
                {Statuses.Taunt, tauntIcon },
                {Statuses.CantAttack, cantAttackIcon },
                {Statuses.DyadStrike, dyadStrikeIcon },
                {Statuses.Silence, silenceIcon },
                {Statuses.hasDeathEvent, deathIcon },
                {Statuses.hasEvent, eventIcon },
                {Statuses.isRanged, rangeIcon },
                {Statuses.hasAura, auraIcon },
                {Statuses.Flying, flyingIcon },
                {Statuses.Cleave, cleaveIcon },
                {Statuses.Piercing, piercingIcon },
            };

            var meshRenderer = model.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null)
            {
                var skinnedMeshRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
                meshMaterial = skinnedMeshRenderer.material;
            }
            else
            {
                meshMaterial = meshRenderer.material;
            }

            highlight = model.GetComponentInChildren<Highlighter>();
            highlight.seeThrough = true;
            highlight.occluder = true;
            highlight.enabled = false;

            attackText.text = piece.attack.ToString();
            healthText.text = piece.health.ToString();

            //rotate to model direction
            model.gameObject.transform.rotation = Quaternion.Euler(DirectionAngle.angle[piece.direction]);

            var colliders = model.GetComponentsInChildren<MeshCollider>();
            if (colliders == null || colliders.Length == 0)
            {
                Debug.LogError(piece.cardTemplateId + " piece missing mesh collider");
            }

            Vector3 topVertex = new Vector3(0, float.NegativeInfinity, 0);
            MeshCollider topCollider = null;
            //need to loop through all colliders (mainly for robots with multiple meshes) to find the top vertex
            foreach (var collider in colliders)
            {
                //find top of the mesh and adjust the hpbar to be just above it
                var rotation = collider.transform.localRotation;

                Vector3[] verts = collider.sharedMesh.vertices;
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 vert = rotation * verts[i];
                    if (vert.y > topVertex.y)
                    {
                        topVertex = vert;
                    }
                }
                topVertex = topVertex + collider.transform.localPosition;
                topCollider = collider;
            }

            //walk up the parent chain (stopping at the Model node) to find the total combined scale.  
            //Realistically it should only be one intermediate scaling anything
            //Have to use the top collider as the scalar
            Vector3 combinedScale = topCollider.transform.localScale;
            var curTransform = topCollider.transform;
            while (curTransform != null && curTransform.name != "Model")
            {
                curTransform = curTransform.parent;
                combinedScale.Scale(curTransform.localScale);
            }

            topVertex.Scale(combinedScale);

            hpBarContainer.transform.localPosition = hpBarContainer.transform.localPosition.SetY(
                topVertex.y + 0.25f
            );
            //Debug.Log(string.Format("{0} top vert y {1} hpbar pos {2}", piece.cardTemplateId, topVertex.y, hpBarContainer.transform.localPosition.y));

            UpdateHpBar();
        }

        void Update()
        {
            if(piece == null) return;

            faceCameraContainer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

            var canAttack = piece.canAttack;
            var canMove = piece.canMove;

            var hpBarTargetColor = Colors.invisible;
            var hpBarPulse = false;

            //Figure out what color the hp bar should be 
            if (piece.currentPlayerHasControl) {
                hpBarTargetColor = hpBarFillFriendlyColor;

                if (canMove || (canAttack && enemiesInRange))
                {
                    hpBarPulse = true;
                }
            }else{
                hpBarTargetColor = hpBarFillEnemyColor;
            }

            if (targetCandidate)
            {
                hpBarTargetColor = Colors.targetOutlineColor;
                hpBarPulse = true;
            }
            else if (piece.isSelected)
            {
                hpBarTargetColor = Colors.selectedOutlineColor;
            }

            //Set it, and make it pulse if it needs to and the value changed
            if(hpBarTargetColor != Colors.invisible){
                hpBarRenderer.color = hpBarTargetColor;
            }

            if(hpBarPulse != prevHpBarPulsing || hpBarTargetColor != prevHpBarColor){
                if(hpBarPulse){
                    var targetColor = hpBarTargetColor.ToHSV();
                    targetColor.V += 0.5f;
                    targetColor.S += 0.2f;
                    targetColor.H -= 0.08f;

                    var outlineColor = targetColor.ToColor();
                    outlineColor.a = 0.3f;
                    hpBarOutlineRenderer.color = outlineColor;

                    Destroy(hpBarOutlineRenderer.GetComponent<iTween>());
                    hpBarOutlineRenderer.gameObject.ColorTo( targetColor.ToColor(), 0.5f, 0f, LoopType.pingPong);
                }else{
                    Destroy(hpBarOutlineRenderer.GetComponent<iTween>());
                    hpBarOutlineRenderer.color = Colors.invisible;
                }
            }

            prevHpBarPulsing = hpBarPulse;
            prevHpBarColor = hpBarTargetColor;

            //Update the attack and move indicators as well
            canAttackIndicator.SetActive(canAttack);
            canMoveIndicator.SetActive(canMove);
        }


        private const int hpBarHpCuttoff = 14;
        public void UpdateHpBar()
        {
            //gotta swap out the bar if it's an enemy for now
            var fillColor = hpBarFillFriendlyColor;
            if(piece.isSelected)
            {
                fillColor = Colors.selectedOutlineColor;
            } 
            else if (!piece.currentPlayerHasControl)
            {
                fillColor = hpBarFillEnemyColor;
            }

            //hpBarFillRenderer.material.SetColor("_Color", fillColor);
            hpBarRenderer.color = fillColor;

            //Not sure exactly why I need to make this null check but on OSX you sometimes get a NRE here
            if(hpBarFillRenderer.material != null)
            {
                hpBarFillRenderer.material.SetFloat("_CurrentHp", piece.health);
                hpBarFillRenderer.material.SetFloat("_MaxHp", piece.maxBuffedHealth);

                if (piece.baseHealth > hpBarHpCuttoff)
                {
                    hpBarFillRenderer.material.SetColor("_LineColor", Color.white);
                }
            }
        }

        //Brute force changing all the sorting orders for things that need to appear in front of the other hp bar shiet
        //Reset everything back to normal once this piece isn't selected anymore
        public void UpdateSelection(bool isSelected)
        {
            int sortChange = isSelected ? 10 : -10;

            attackText.sortingOrder += sortChange;
            healthText.sortingOrder += sortChange;
            armorText.sortingOrder += sortChange;
            hpBarRenderer.sortingOrder += sortChange;
            hpBarFillRenderer.sortingOrder += sortChange;
            canAttackRenderer.sortingOrder += sortChange;
            canMoveRenderer.sortingOrder += sortChange;

            //also all the status icons
            foreach(var icon in statusIcons.Values){
                var iconRenderers = icon.GetComponents<SVGRenderer>();
                foreach(var renderer in iconRenderers){
                    renderer.sortingOrder += sortChange;
                }
            }
        }

        public void UpdateTurn()
        {
            UpdateHpBar();
        }

        public class SpawnAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get; set; }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public MapModel map { get; set; }
            public IMapService mapService { get; set; }

            private Vector3 destPosition { get; set; }
            private Vector3 startOffset = new Vector3(0, 8f, 0);
            private float dropTime = 0.7f;
            private float pieceMagnitude = 0f;
            private float pieceDuration = 0f;

            //private Tile originTile { get; set; }
            private List<Tile> oneRing { get; set; }
            private List<Tile> twoRing { get; set; }

            public void Init()
            {

                destPosition = map.tiles[piece.piece.tilePosition].fullPosition;
                piece.gameObject.transform.position = destPosition + startOffset;

                pieceMagnitude = Math.Min(1f, (piece.piece.health + piece.piece.attack) / 20f);
                pieceDuration = Math.Min(1f, (piece.piece.health + piece.piece.attack) / 10f);

                //originTile = map.tiles[piece.piece.tilePosition];
                oneRing = mapService.GetKingTilesInRadius(piece.piece.tilePosition, 1).Values.ToList();
                twoRing = mapService.GetKingTilesInRadius(piece.piece.tilePosition, 2).Values.Except(oneRing).ToList();

                iTweenExtensions.MoveTo(piece.gameObject, destPosition, dropTime, 0, EaseType.easeInQuart);
            }
            public void Update()
            {
                //weird case if the piece died while spawning like the phantom piece when the action gets cancelled
                if (piece == null || piece.gameObject == null)
                {
                    Complete = true;
                    return;
                }

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

                    var remainingTween = piece.gameObject.GetComponent<iTween>();
                    if (remainingTween != null)
                    {
                        Destroy(remainingTween);
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

            public void Init() {
                iTweenExtensions.RotateTo(piece.model.gameObject, destAngle, rotateSpeed, 0f);
            }
            public void Update()
            {
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
            public bool Async { get { return isTeleport; } }
            public float? postDelay { get { return null; } }

            public PieceModel piece { get; set; }
            public Animator anim { get; set; }
            public PieceFinishedMovingSignal finishedMoving { get; set; }
            public Vector3 destination { get; set; }
            public bool isTeleport { get; set; }

            private float moveTime = 0.33f;
            Vector3 curveHeight = new Vector3(0, 0.20f, 0);
            private float curveMult = 1.0f;
            private BezierSpline moveSpline;
            private SplineWalker walker;

            public void Init()
            {
                moveSpline = GameObject.Find("PieceMoveSpline").GetComponent<BezierSpline>();

                piece.isMoving = true;
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

                if (anim != null)
                {
                    anim.SetTrigger("onMove");
                }
            }
            public void Update()
            {
                if(walker.progress > 0.99 && !Complete)
                {
                    piece.gameObject.transform.position = destination;
                    piece.isMoving = false;
                    Complete = true;
                    Destroy(walker);
                    finishedMoving.Dispatch(piece);
                }
            }
        }

        public class EventTriggerAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public string eventName { get; set; }

            public void Init() { }
            public void Update()
            {
                if (piece.anim != null && !string.IsNullOrEmpty(eventName))
                {
                    piece.anim.SetTrigger(eventName);
                    Complete = true;
                }
            }
        }

        public class TakeDamageAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return 0.3f; } }

            public Transform parent { get; set; }
            public GameObject numberSplat { get; set; }

            public int change { get; set; }
            public int? bonus { get; set; }
            public string bonusMsg { get; set; }

            public void Init() { }
            public void Update()
            {
                var newNumberSplat = Instantiate(numberSplat, parent, true) as GameObject;
                newNumberSplat.transform.localPosition = new Vector3(0, 0.5f, -0.85f);
                var view = newNumberSplat.GetComponent<NumberSplatView>();
                view.change = change;
                view.bonus = bonus;
                view.bonusText = bonusMsg;
                view.animate = true;

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

            public void Init() {
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
            }
            public void Update()
            {
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

        public class UpdateHpBarAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }

            public void Init() { }
            public void Update()
            {
                piece.UpdateHpBar();
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
            public Animator anim { get; set; }

            //different animations
            public bool isExact { get; set; }
            public bool isBig { get; set; }

            public void Init() {
                if (anim != null)
                {
                    if (isBig)
                    {
                        anim.SetTrigger("onBigDeath");
                    }
                    else if (isExact)
                    {
                        anim.SetTrigger("onExactDeath");
                    }
                    else
                    {
                        anim.SetTrigger("onDeath");
                    }
                }
                iTweenExtensions.ScaleTo(piece.gameObject, Vector3.zero, 1.5f, 0, EaseType.easeInQuart);
            }
            public void Update()
            {
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

        public class ChangeStatusAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceStatusChangeModel pieceStatusChange { get; set; }
            public PieceView pieceView { get; set; }

            private float transitionTime = 1f;
            private float timeAccum = 0f;

            private void AnimInStatus(GameObject statusIcon)
            {
                statusIcon.transform.localScale = Vector3.zero;
                statusIcon.SetActive(true);
                iTweenExtensions.ScaleTo(statusIcon, Vector3.one, transitionTime, 0f, EaseType.easeInOutBounce);
            }

            private void AnimOutStatus(GameObject statusIcon)
            {
                statusIcon.transform.localScale = Vector3.one;
                iTweenExtensions.ScaleTo(statusIcon, Vector3.zero, transitionTime, 0f, EaseType.easeInExpo);
                //set active false in update
            }

            public void Init()
            {
                //add 
                if (pieceStatusChange.add.HasValue && !FlagsHelper.IsSet(pieceStatusChange.add.Value, Statuses.None))
                {
                    foreach (var statusIcon in pieceView.statusIcons)
                    {
                        if (FlagsHelper.IsSet(pieceStatusChange.add.Value, statusIcon.Key))
                        {
                            AnimInStatus(statusIcon.Value);
                        }
                    }

                    if (FlagsHelper.IsSet(pieceStatusChange.add.Value, Statuses.Shield))
                    {
                        //TODO: animate shield coming up with scale tween
                        pieceView.shield.SetActive(true);
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.add.Value, Statuses.Paralyze))
                    {
                        pieceView.paralyze.transform.localScale = Vector3.one;
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.add.Value, Statuses.Cloak))
                    {
                        pieceView.cloak.transform.localScale = Vector3.one;
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.add.Value, Statuses.Root))
                    {
                        pieceView.root.SetActive(true);
                    }
                }
                //remove
                if (pieceStatusChange.remove.HasValue && !FlagsHelper.IsSet(pieceStatusChange.remove.Value, Statuses.None))
                {
                    foreach (var statusIcon in pieceView.statusIcons)
                    {
                        if (FlagsHelper.IsSet(pieceStatusChange.remove.Value, statusIcon.Key))
                        {
                            AnimOutStatus(statusIcon.Value);
                        }
                    }

                    if (FlagsHelper.IsSet(pieceStatusChange.remove.Value, Statuses.Shield))
                    {
                        pieceView.shield.SetActive(false);
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.remove.Value, Statuses.Paralyze))
                    {
                        pieceView.paralyze.transform.localScale = Vector3.zero;
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.remove.Value, Statuses.Cloak))
                    {
                        pieceView.cloak.transform.localScale = Vector3.zero;
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.remove.Value, Statuses.Root))
                    {
                        pieceView.root.SetActive(false);
                    }
                }
            }
            public void Update()
            {
                timeAccum += Time.deltaTime;

                if (timeAccum > transitionTime)
                {
                    //disable any icons that were removed
                    if (pieceStatusChange.remove.HasValue && !FlagsHelper.IsSet(pieceStatusChange.remove.Value, Statuses.None))
                    {
                        foreach (var statusIcon in pieceView.statusIcons)
                        {
                            if (FlagsHelper.IsSet(pieceStatusChange.remove.Value, statusIcon.Key))
                            {
                                statusIcon.Value.SetActive(false);
                            }
                        }
                    }

                    Complete = true;
                }
            }
        }
    }
}

using ctac.signals;
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
        public SVGAsset hpBarSvg;
        public SVGAsset hpBarSvgEnemy;
        public SVGRenderer hpBarSvgRenderer;
        public MeshRenderer hpBarFillRenderer;
        private Color32 hpBarFillFriendlyColor = Colors.friendlyColor;
        private Color32 hpBarFillEnemyColor = Colors.enemyColor;

        public GameObject faceCameraContainer;
        public GameObject eventIconContainer;
        public GameObject circleBg;
        public GameObject deathIcon;
        public GameObject eventIcon;
        public GameObject rangeIcon;
        public GameObject auraIcon;
        public GameObject model;
        private GameObject textContainer;
        public bool targetCandidate = false;
        public bool enemiesInRange = false;

        public Material meshMaterial;
        private Highlighter highlight;
        private float outlineWidth = 0.01f;

        public Animator anim;

        protected override void Start()
        {
            model = piece.gameObject.transform.FindChild("Model").gameObject;
            faceCameraContainer = piece.gameObject.transform.FindChild("FaceCameraContainer").gameObject;
            anim = piece.gameObject.GetComponentInChildren<Animator>();

            hpBarContainer = faceCameraContainer.transform.FindChild("HpBarContainer").gameObject;
            hpBar = hpBarContainer.transform.FindChild("hpbar").gameObject;
            hpBarfill = hpBarContainer.transform.FindChild("HpBarFill").gameObject;
            hpBarFillRenderer = hpBarfill.GetComponent<MeshRenderer>();
            hpBarSvgRenderer = hpBar.GetComponent<SVGRenderer>();
            hpBarSvgEnemy = loader.Load<SVGAsset>("UI/hpbar enemy");
            hpBarSvg = loader.Load<SVGAsset>("UI/hpbar");

            textContainer = hpBarContainer.transform.FindChild("TextContainer").gameObject;
            attackGO = textContainer.transform.FindChild("Attack").gameObject;
            healthGO = textContainer.transform.FindChild("Health").gameObject;
            armorGO = textContainer.transform.FindChild("Armor").gameObject;
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            armorText = armorGO.GetComponent<TextMeshPro>();
            shield = faceCameraContainer.transform.FindChild("Shield").gameObject;
            cloak = faceCameraContainer.transform.FindChild("Cloak").gameObject;
            paralyze = faceCameraContainer.transform.FindChild("Paralyze").gameObject;
            root = faceCameraContainer.transform.FindChild("Root").gameObject;
            armorBG = faceCameraContainer.transform.FindChild("Armor").gameObject;

            eventIconContainer = faceCameraContainer.transform.FindChild("EventIconContainer").gameObject;
            circleBg = eventIconContainer.transform.FindChild("CircleBg").gameObject;
            eventIcon = eventIconContainer.transform.FindChild("Event").gameObject;
            deathIcon = eventIconContainer.transform.FindChild("Death").gameObject;
            rangeIcon = eventIconContainer.transform.FindChild("Range").gameObject;
            auraIcon = eventIconContainer.transform.FindChild("Aura").gameObject;

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

            circleBg.SetActive(false);
            deathIcon.SetActive(false);
            eventIcon.SetActive(false);
            rangeIcon.SetActive(false);
            auraIcon.SetActive(false);

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

            if (targetCandidate)
            {
                highlight.enabled = true;
                highlight.ConstantOn(Colors.targetOutlineColor);
                meshMaterial.SetColor("_OutlineColor", Colors.targetOutlineColor);
                meshMaterial.SetFloat("_Outline", outlineWidth);
            }
            else if (piece.isSelected)
            {
                highlight.enabled = true;
                highlight.ConstantOn(Colors.selectedOutlineColor);
                meshMaterial.SetColor("_OutlineColor", Colors.selectedOutlineColor);
                meshMaterial.SetFloat("_Outline", outlineWidth);
            }else if (piece.currentPlayerHasControl) {

                if (piece.canMove && piece.canAttack)
                {
                    highlight.enabled = true;
                    highlight.ConstantOn(Colors.moveAttackOutlineColor);
                    meshMaterial.SetColor("_OutlineColor", Colors.moveAttackOutlineColor);
                    meshMaterial.SetFloat("_Outline", outlineWidth);
                }
                else if (piece.canAttack && enemiesInRange)
                {
                    highlight.enabled = true;
                    highlight.ConstantOn(Colors.attackOutlineColor);
                    meshMaterial.SetColor("_OutlineColor", Colors.attackOutlineColor);
                    meshMaterial.SetFloat("_Outline", outlineWidth);
                }
                else if (piece.canMove)
                {
                    highlight.enabled = true;
                    highlight.ConstantOn(Colors.moveOutlineColor);
                    meshMaterial.SetColor("_OutlineColor", Colors.moveOutlineColor);
                    meshMaterial.SetFloat("_Outline", outlineWidth);
                }
                else
                {
                    highlight.ConstantOff();
                    highlight.enabled = false;
                    meshMaterial.SetColor("_OutlineColor", Color.black);
                    meshMaterial.SetFloat("_Outline", outlineWidth);
                }
            }
            else
            {
                highlight.ConstantOff();
                highlight.enabled = false;
                meshMaterial.SetColor("_OutlineColor", Color.black);
                meshMaterial.SetFloat("_Outline", outlineWidth);
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
            rangeIcon.SetActive(false);
            auraIcon.SetActive(false);
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
            else if (piece.isRanged)
            {
                //TODO: need to do something better for ranged units that also have events
                circleBg.SetActive(true);
                rangeIcon.SetActive(true);
            } else if (piece.hasAura) {
                circleBg.SetActive(true);
                auraIcon.SetActive(true);
            }
        }


        private const int hpBarHpCuttoff = 14;
        public void UpdateHpBar()
        {
            //gotta swap out the bar if it's an enemy for now
            var fillColor = hpBarFillFriendlyColor;
            if (!piece.currentPlayerHasControl)
            {
                hpBarSvgRenderer.vectorGraphics = hpBarSvgEnemy;
                fillColor = hpBarFillEnemyColor;
            }
            else
            {
                hpBarSvgRenderer.vectorGraphics = hpBarSvg;
            }

            hpBarFillRenderer.material.SetColor("_Color", fillColor);
            hpBarFillRenderer.material.SetFloat("_CurrentHp", piece.health);
            hpBarFillRenderer.material.SetFloat("_MaxHp", piece.maxBuffedHealth);

            if (piece.baseHealth > hpBarHpCuttoff)
            {
                hpBarFillRenderer.material.SetColor("_LineColor", fillColor);
            }
        }

        public void UpdateTurn()
        {
            //gotta swap out the bar if it's an enemy for now
            if (!piece.currentPlayerHasControl)
            {
                hpBarSvgRenderer.vectorGraphics = hpBarSvgEnemy;
                hpBarFillRenderer.material.SetColor("_Color", hpBarFillEnemyColor);

                if (piece.baseHealth > hpBarHpCuttoff)
                {
                    hpBarFillRenderer.material.SetColor("_LineColor", hpBarFillEnemyColor);
                }
            }
            else
            {
                hpBarSvgRenderer.vectorGraphics = hpBarSvg;
                hpBarFillRenderer.material.SetColor("_Color", hpBarFillFriendlyColor);
                if (piece.baseHealth > hpBarHpCuttoff)
                {
                    hpBarFillRenderer.material.SetColor("_LineColor", hpBarFillFriendlyColor);
                }
            }
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
                //weird case if the piece died while spawning like the phantom piece when the action gets cancelled
                if (piece == null || piece.gameObject == null)
                {
                    Complete = true;
                    return;
                }

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

                //if (Vector3.Distance(piece.gameObject.transform.position, destination) < 0.01)
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
            }
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

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

        private MeshRenderer meshRenderer;
        private Highlighter highlight;
        private float outlineWidth = 0.01f;
        private Color targetOutlineColor = ColorExtensions.HexToColor("E1036C");
        private Color moveAttackOutlineColor = ColorExtensions.HexToColor("63FF32");
        private Color moveOutlineColor = ColorExtensions.HexToColor("006BFF");
        private Color attackOutlineColor = ColorExtensions.HexToColor("FF5E2E");
        private Color selectedOutlineColor = ColorExtensions.HexToColor("DBFF00");

        protected override void Start()
        {
            model = piece.gameObject.transform.FindChild("Model").gameObject;
            faceCameraContainer = piece.gameObject.transform.FindChild("FaceCameraContainer").gameObject;

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

            meshRenderer = model.GetComponentInChildren<MeshRenderer>();
            highlight = model.GetComponentInChildren<Highlighter>();

            attackText.text = piece.attack.ToString();
            healthText.text = piece.health.ToString();

            circleBg.SetActive(false);
            deathIcon.SetActive(false);
            eventIcon.SetActive(false);
            rangeIcon.SetActive(false);
            auraIcon.SetActive(false);

            //rotate to model direction
            model.gameObject.transform.rotation = Quaternion.Euler(DirectionAngle.angle[piece.direction]);

            //find top of the mesh and adjust the hpbar to be just above it
            Vector3[] verts = model.GetComponentInChildren<MeshFilter>().sharedMesh.vertices;
            Vector3 topVertex = new Vector3(0, float.NegativeInfinity, 0);
            for (int i = 0; i < verts.Length; i++)
            {
                //Vector3 vert = transform.TransformPoint(verts[i]);
                Vector3 vert = verts[i];
                if (vert.y > topVertex.y)
                {
                    topVertex = vert;
                }
            }
            hpBarContainer.transform.localPosition = hpBarContainer.transform.localPosition.SetY(
                topVertex.y * 1.5f + 0.9f
            );

            UpdateHpBar();
        }

        void Update()
        {
            if(piece == null) return;

            faceCameraContainer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

            if (targetCandidate)
            {
                highlight.ConstantOn(targetOutlineColor);
                meshRenderer.material.SetColor("_OutlineColor", targetOutlineColor);
                meshRenderer.material.SetFloat("_Outline", outlineWidth);
            }
            else if (piece.isSelected)
            {
                highlight.ConstantOn(selectedOutlineColor);
                meshRenderer.material.SetColor("_OutlineColor", selectedOutlineColor);
                meshRenderer.material.SetFloat("_Outline", outlineWidth);
            }else if (piece.currentPlayerHasControl) {

                if (piece.canMove && piece.canAttack)
                {
                    highlight.ConstantOn(moveAttackOutlineColor);
                    meshRenderer.material.SetColor("_OutlineColor", moveAttackOutlineColor);
                    meshRenderer.material.SetFloat("_Outline", outlineWidth);
                }
                else if (piece.canAttack && enemiesInRange)
                {
                    highlight.ConstantOn(attackOutlineColor);
                    meshRenderer.material.SetColor("_OutlineColor", attackOutlineColor);
                    meshRenderer.material.SetFloat("_Outline", outlineWidth);
                }
                else if (piece.canMove)
                {
                    highlight.ConstantOn(moveOutlineColor);
                    meshRenderer.material.SetColor("_OutlineColor", moveOutlineColor);
                    meshRenderer.material.SetFloat("_Outline", outlineWidth);
                }
                else
                {
                    highlight.ConstantOff();
                    meshRenderer.material.SetColor("_OutlineColor", Color.black);
                    meshRenderer.material.SetFloat("_Outline", outlineWidth);
                }
            }
            else
            {
                highlight.ConstantOff();
                meshRenderer.material.SetColor("_OutlineColor", Color.black);
                meshRenderer.material.SetFloat("_Outline", outlineWidth);
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
            else if (piece.range.HasValue)
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
                if(walker.progress > 0.99 && !Complete)
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
                newNumberSplat.transform.localPosition = new Vector3(0, 0.5f, -0.5f);
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

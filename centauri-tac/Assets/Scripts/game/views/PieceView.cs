using ctac.signals;
using strange.extensions.mediation.impl;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ctac.util;

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
        public List<Tile> attackRangeTiles = null; //Should be able to attack any enemy piece within these tiles
        public bool enemiesInRange = false;
        public bool hovered = false;

        public List<Material> meshMaterials = new List<Material>();
        private Dictionary<Statuses, GameObject> statusIcons;
        private float idealHpBarYPos = 0f;

        public Animator anim;

        protected override void Start()
        {
            model = piece.gameObject.transform.Find("Model").gameObject;
            faceCameraContainer = piece.gameObject.transform.Find("FaceCameraContainer").gameObject;
            anim = piece.gameObject.GetComponentInChildren<Animator>();

            hpBarContainer = piece.gameObject.transform.transform.Find("HpBarContainer").gameObject;
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

            //Get all the materials (which should be instances of the piece shader except in edge cases for sub parts or glowing things)
            //Make sure the outline color of the shared material is transparent so the piece doesn't occlude itself when we set the outline color
            //to be friendly or enemy
            var meshRenderers = model.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null)
            {
                foreach(var meshRenderer in meshRenderers){
                    meshMaterials.Add(meshRenderer.material);
                    foreach(var sharedMat in meshRenderer.sharedMaterials){
                        sharedMat.SetColor("_OutlineColor", Colors.transparentWhite);
                    }
                }
            }
            var skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            if(skinnedMeshRenderers != null)
            {
                foreach(var meshRenderer in skinnedMeshRenderers){
                    meshMaterials.Add(meshRenderer.material);
                    foreach(var sharedMat in meshRenderer.sharedMaterials){
                        sharedMat.SetColor("_OutlineColor", Colors.transparentWhite);
                    }
                }
            }

            attackText.text = piece.attack.ToString();
            healthText.text = piece.health.ToString();

            //rotate to model direction
            model.gameObject.transform.rotation = Quaternion.Euler(DirectionHelpers.directionAngle[piece.direction]);

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
                topVertex.y + 0.15f
            );
            idealHpBarYPos = hpBarContainer.transform.localPosition.y;
            //Debug.Log(string.Format("{0} top vert y {1} hpbar pos {2}", piece.cardTemplateId, topVertex.y, hpBarContainer.transform.localPosition.y));

            UpdateHpBar();
        }

        void Update()
        {
            if(piece == null) return;

            var cameraRot = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            faceCameraContainer.transform.rotation = cameraRot;
            hpBarContainer.transform.rotation = cameraRot;

            //As the camera zooms out, increase the size of the hpbar and faceCamera stuff so they're still ledgible
            var cameraScale = Vector3.one * Mathf.Clamp(Camera.main.orthographicSize * 0.25f , 1, 3f);
            faceCameraContainer.transform.localScale = cameraScale;
            hpBarContainer.transform.localScale = cameraScale;
            //slightly bump up the position of the hp bar as you zoom out so it doesn't overlap
            hpBarContainer.transform.localPosition = hpBarContainer.transform.localPosition.SetY(idealHpBarYPos + faceCameraContainer.transform.localScale.x * 0.1f);

            UpdateHpBarColors();
        }


        private Color prevOutlineColorTo = Colors.invisible;
        private bool reverseLoop = false;
        private float runningTime = 0f;
        public void UpdateHpBarColors()
        {
            var canAttack = piece.canAttack;
            var canMove = piece.canMove;

            var hpBarTargetColor = Colors.invisible;
            var outlineColorFrom = Colors.invisible;
            var outlineColorTo = Colors.invisible;

            //Figure out what color the hp bar should be
            if (piece.currentPlayerHasControl) {
                hpBarTargetColor = hpBarFillFriendlyColor;

                if (canMove)
                {
                    outlineColorTo = Colors.canMoveColor;
                }
                if (canAttack && enemiesInRange)
                {
                    if(canMove)
                    {
                        outlineColorFrom = Colors.canAttackColor;
                    }else{
                        outlineColorTo = Colors.canAttackColor;
                    }
                }
            }else{
                hpBarTargetColor = hpBarFillEnemyColor;
            }

            if (targetCandidate)
            {
                hpBarTargetColor = piece.currentPlayerHasControl ? Colors.friendlyTargetOutlineColor : Colors.enemyTargetOutlineColor;
                outlineColorTo = hpBarTargetColor;
            }
            else if (piece.isSelected)
            {
                hpBarTargetColor = Colors.selectedOutlineColor;
            }
            else if(piece.tags.Contains(Constants.targetPieceTag))
            {
                hpBarTargetColor = Colors.ghostPieceColor;
            }

            if(hovered){
                hpBarTargetColor -= Colors.hoverTint;
            }

            if(hpBarTargetColor != Colors.invisible){
                hpBarRenderer.color = hpBarTargetColor;
            }

            //default color from value if we need to pulse the outline and don't have one explicitly set
            if(outlineColorTo != Colors.invisible && outlineColorFrom == Colors.invisible){
                outlineColorFrom = new Color(hpBarTargetColor.r, hpBarTargetColor.g, hpBarTargetColor.b, 0.3f);
            }

            //Should we be pulsing the outline
            if(outlineColorTo != Colors.invisible){
                const float time = 0.5f;
                if(prevOutlineColorTo != outlineColorTo){
                    //newly starting to pulse;
                    runningTime = 0f;
                }

	            runningTime += Time.deltaTime;
                var percentage = 0f;
                if(reverseLoop){
                    percentage = 1 - runningTime/time;
                }else{
                    percentage = runningTime/time;
                }

                hpBarOutlineRenderer.color = new Color(
                    Mathf.Lerp(outlineColorFrom.r, outlineColorTo.r, percentage),
                    Mathf.Lerp(outlineColorFrom.g, outlineColorTo.g, percentage),
                    Mathf.Lerp(outlineColorFrom.b, outlineColorTo.b, percentage),
                    Mathf.Lerp(outlineColorFrom.a, outlineColorTo.a, percentage)
                );

                if(percentage >= 1f || percentage <= 0f){
                    reverseLoop = !reverseLoop;
                    runningTime = 0f;
                }
            }else{
                hpBarOutlineRenderer.color = Colors.invisible;
            }

            prevOutlineColorTo = outlineColorTo;

            //Update the attack and move indicators as well
            canAttackIndicator.SetActive(canAttack);
            canMoveIndicator.SetActive(canMove);
        }

        private const int hpBarHpCuttoff = 14;
        public void UpdateHpBar()
        {
            //Not sure exactly why I need to make this null check but on OSX you sometimes get a NRE here
            if(hpBarFillRenderer != null && hpBarFillRenderer.material != null)
            {
                hpBarFillRenderer.material.SetFloat("_CurrentHp", piece.health);
                hpBarFillRenderer.material.SetFloat("_MaxHp", piece.maxBuffedHealth);

                if (piece.baseHealth > hpBarHpCuttoff)
                {
                    hpBarFillRenderer.material.SetColor("_LineColor", Color.white);
                }
            }

            //also update the pieces occlusion color based on if they're friendly or not
            foreach(var meshMaterial in meshMaterials){
                meshMaterial.SetColor("_OutlineColor", piece.currentPlayerHasControl ? Colors.friendlyColor : Colors.enemyColor);
            }
        }

        //Brute force changing all the sorting orders for things that need to appear in front of the other hp bar shiet
        //Reset everything back to normal once this piece isn't selected anymore
        public void FocusHpBar(bool isSelected, int extraBoost = 0)
        {
            int sortChange = isSelected ? 10 + extraBoost : -10 - extraBoost;

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

        public interface IPieceAnimate : IAnimate
        {
            PieceView piece { get; }
        }

        public class SpawnAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get; set; }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public MapModel map { get; set; }
            public TraumaModel trauma { get; set; }
            public IMapService mapService { get; set; }
            public IResourceLoaderService loader { get; set; }
            public ISoundService sounds { get; set; }

            private Vector3 destPosition { get; set; }
            private Vector3 startOffset = new Vector3(0, 8f, 0);
            private float dropTime = 0.7f;
            private float pieceMagnitude = 0f;
            private float pieceDuration = 0f;

            //private Tile originTile { get; set; }
            private List<Tile> oneRing { get; set; }
            private List<Tile> twoRing { get; set; }
            private GameObject sleepingStatusParticle;

            public void Init()
            {
                sleepingStatusParticle = loader.Load<GameObject>("Particles/Sleeping Status");

                destPosition = map.tiles[piece.piece.tilePosition].fullPosition;
                piece.gameObject.transform.position = destPosition + startOffset;

                pieceMagnitude = Math.Min(1f, (piece.piece.health + piece.piece.attack) / 20f);
                pieceDuration = Math.Min(1f, (piece.piece.health + piece.piece.attack) / 10f);

                //originTile = map.tiles[piece.piece.tilePosition];
                oneRing = mapService.GetKingTilesInRadius(piece.piece.tilePosition, 1).Values.ToList();
                twoRing = mapService.GetKingTilesInRadius(piece.piece.tilePosition, 2).Values.Except(oneRing).ToList();

                iTweenExtensions.MoveTo(piece.gameObject, destPosition, dropTime, 0, EaseType.easeInQuart);
                sounds.PlaySound("SpawnStart");
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
                    trauma.trauma += pieceMagnitude * 2;
                    sounds.PlaySound("SpawnEnd");
                    piece.CreatePieceParticle(loader, "Particles/Spawn Smoke", Vector3.zero);

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

                    piece.CreateSleepingStatus(sleepingStatusParticle);
                }
            }
        }


        public class RotateAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public Vector3 destAngle { get; set; }
            private float rotateSpeed = 0.6f;

            public void Init() {
                iTweenExtensions.RotateTo(piece.model.gameObject, destAngle, rotateSpeed, 0f);
            }
            public void Update()
            {
                Complete = true;
            }
        }

        public class MoveAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return isTeleport; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
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

                piece.piece.isMoving = true;
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
                    piece.piece.isMoving = false;
                    Complete = true;
                    Destroy(walker);
                    finishedMoving.Dispatch(piece.piece);
                }
            }
        }

        public class EventTriggerAnim : IPieceAnimate
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
                newNumberSplat.transform.localRotation = Quaternion.identity;
                var view = newNumberSplat.GetComponent<NumberSplatView>();
                view.change = change;
                view.bonus = bonus;
                view.bonusText = bonusMsg;
                view.animate = true;

                Complete = true;
            }
        }

        public class UpdateTextAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceTextAnimationFinishedSignal animFinished { get; set; }
            public PieceView piece { get; set; }
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
                animFinished.Dispatch(piece.piece);
            }
        }

        public class UpdateArmorAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
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

        public class UpdateHpBarAnim : IPieceAnimate
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

        public class CharmAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public IResourceLoaderService loader { get; set; }

            GameObject sleepingStatusParticle;

            public void Init() {
                sleepingStatusParticle = loader.Load<GameObject>("Particles/Sleeping Status");
            }
            public void Update()
            {
                piece.UpdateHpBar();
                piece.CreateSleepingStatus(sleepingStatusParticle);
                Complete = true;
            }
        }

        public class DieAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceDiedSignal pieceDied { get; set; }
            public PieceView piece { get; set; }
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
                    pieceDied.Dispatch(piece.piece);
                }
            }
        }

        public class UnsummonAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return null; } }

            public PieceView piece { get; set; }
            public PieceDiedSignal pieceDied { get; set; }

            private Vector3 destPosition { get; set; }
            private Vector3 endOffset = new Vector3(0, 8f, 0);
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

        public class ChangeStatusAnim : IPieceAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public PieceStatusChangeModel pieceStatusChange { get; set; }
            public PieceView piece { get; set; }
            public IResourceLoaderService loader { get; set; }

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
                var anim = piece.anim;
                //add
                if (pieceStatusChange.add.HasValue && (pieceStatusChange.add.Value != Statuses.None))
                {
                    foreach (var statusIcon in piece.statusIcons)
                    {
                        if ((pieceStatusChange.add.Value & statusIcon.Key) != 0)
                        {
                            AnimInStatus(statusIcon.Value);
                        }
                    }

                    if ((pieceStatusChange.add.Value & Statuses.Shield) != 0)
                    {
                        //TODO: animate shield coming up with scale tween
                        piece.shield.SetActive(true);
                    }
                    if ((pieceStatusChange.add.Value & Statuses.Petrify) != 0)
                    {
                        foreach(var mat in piece.meshMaterials){
                            mat.SetFloat("_IsParalyzed", 1f);
                        }
                        anim.speed = 0;
                    }
                    if ((pieceStatusChange.add.Value & Statuses.Cloak) != 0)
                    {
                        piece.CreatePieceParticle(loader, Constants.StatusParticleResources[Statuses.Cloak], Vector3.zero);
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.add.Value, Statuses.Root))
                    {
                        piece.root.SetActive(true);
                    }
                    if ((pieceStatusChange.add.Value & Statuses.hasAura) != 0)
                    {
                        piece.CreatePieceParticle(loader, Constants.StatusParticleResources[Statuses.hasAura], Vector3.zero);
                    }
                }
                //remove
                if (pieceStatusChange.remove.HasValue && (pieceStatusChange.remove.Value != Statuses.None))
                {
                    foreach (var statusIcon in piece.statusIcons)
                    {
                        if ((pieceStatusChange.remove.Value & statusIcon.Key) != 0)
                        {
                            AnimOutStatus(statusIcon.Value);
                        }
                    }

                    if ((pieceStatusChange.remove.Value & Statuses.Shield) != 0)
                    {
                        piece.shield.SetActive(false);
                    }
                    if (FlagsHelper.IsSet(pieceStatusChange.remove.Value, Statuses.Petrify))
                    {
                        foreach(var mat in piece.meshMaterials){
                            mat.SetFloat("_IsParalyzed", 0f);
                        }
                        anim.speed = 1;
                    }
                    if ((pieceStatusChange.remove.Value & Statuses.Cloak) != 0)
                    {
                        piece.DestroyPieceParticle(Constants.StatusParticleResources[Statuses.Cloak]);
                    }
                    if ((pieceStatusChange.remove.Value & Statuses.Root) != 0)
                    {
                        piece.root.SetActive(false);
                    }
                    if ((pieceStatusChange.remove.Value & Statuses.hasAura) != 0)
                    {
                        piece.DestroyPieceParticle(Constants.StatusParticleResources[Statuses.hasAura]);
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
                        foreach (var statusIcon in piece.statusIcons)
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

        public void CreateSleepingStatus(GameObject sleepingStatusParticle){

            //Add in the sleeping sickness particles if the piece can't attack which should be everyone except charge
            if(!piece.canAttack){
                var newZs = GameObject.Instantiate(sleepingStatusParticle, new Vector3(0f, 0.3f, -0.15f), Quaternion.Euler(-90f, 0, 45f));
                newZs.transform.SetParent(faceCameraContainer.transform, false);
                newZs.name = "Sleeping Status";
            }
        }

        public void CreatePieceParticle(IResourceLoaderService loaderService, string particleResource, Vector3 localPosition){
            var loadedParticleResource = loader.Load<GameObject>(particleResource);
            var newCloak = GameObject.Instantiate(loadedParticleResource, localPosition, Quaternion.Euler(-90f, 0, 45f));
            newCloak.transform.SetParent(faceCameraContainer.transform, false);
            newCloak.name = particleResource;
        }

        public void DestroyPieceParticle(string particleResource){
            var statusParticle = faceCameraContainer.transform.Find(particleResource);
            if(statusParticle != null){
                var particle = statusParticle.GetComponent<ParticleSystem>();
                if(particle != null){
                    particle.Stop();
                }
                GameObject.Destroy(particle.gameObject, 3f);
            }
        }

    }
}

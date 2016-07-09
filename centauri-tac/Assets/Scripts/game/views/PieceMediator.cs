using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class PieceMediator : Mediator
    {
        [Inject] public PieceView view { get; set; }

        [Inject] public PieceMovedSignal pieceMoved { get; set; }
        [Inject] public PieceAttackedSignal pieceAttacked { get; set; }
        [Inject] public PieceRotatedSignal pieceRotated { get; set; }

        [Inject] public PieceHealthChangedSignal pieceHpChanged { get; set; }
        [Inject] public PieceAttributeChangedSignal pieceAttrChanged { get; set; }
        [Inject] public PieceBuffSignal pieceBuffed { get; set; }
        [Inject] public PieceStatusChangeSignal pieceStatusChanged { get; set; }
        [Inject] public PieceTransformedSignal pieceTransformed { get; set; }
        [Inject] public PieceArmorChangedSignal pieceArmorChanged { get; set; }

        [Inject] public PieceTextAnimationFinishedSignal pieceTextAnimFinished { get; set; }
        [Inject] public PieceFinishedMovingSignal pieceFinishedMoving { get; set; }

        [Inject] public StartSelectTargetSignal startTarget { get; set; }
        [Inject] public SelectTargetSignal targetSelected { get; set; }
        [Inject] public CancelSelectTargetSignal targetCancel { get; set; }

        [Inject] public StartSelectAbilityTargetSignal startAbilityTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal targetAbilitySelected { get; set; }
        [Inject] public CancelSelectAbilityTargetSignal targetAbilityCancel { get; set; }

        [Inject] public PieceDiedSignal pieceDied { get; set; }
        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public AnimationQueueModel animationQueue { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public IMapService mapService { get; set; }

        public override void OnRegister()
        {
            pieceMoved.AddListener(onMove);
            pieceAttacked.AddListener(onAttacked);
            pieceRotated.AddListener(onRotated);
            pieceHpChanged.AddListener(onHealthChange);
            pieceAttrChanged.AddListener(onAttrChange);
            pieceStatusChanged.AddListener(onStatusChange);
            pieceTransformed.AddListener(onTransformed);
            pieceArmorChanged.AddListener(onArmorChange);
            pieceBuffed.AddListener(onBuffed);
            pieceTextAnimFinished.AddListener(onAnimFinished);
            turnEnded.AddListener(onTurnEnded);
            pieceDied.AddListener(onPieceDied);

            startTarget.AddListener(onStartSelectTarget);
            targetSelected.AddListener(onTargetSelected);
            targetCancel.AddListener(onTargetCancel);

            startAbilityTarget.AddListener(onStartSelectAbilityTarget);
            targetAbilitySelected.AddListener(onTargetAbilitySelected);
            targetAbilityCancel.AddListener(onTargetAbilityCancel);
        }

        public override void onRemove()
        {
            pieceMoved.RemoveListener(onMove);
            pieceAttacked.RemoveListener(onAttacked);
            pieceRotated.RemoveListener(onRotated);
            pieceHpChanged.RemoveListener(onHealthChange);
            pieceAttrChanged.RemoveListener(onAttrChange);
            pieceBuffed.RemoveListener(onBuffed);
            pieceStatusChanged.RemoveListener(onStatusChange);
            pieceArmorChanged.RemoveListener(onArmorChange);
            pieceTransformed.RemoveListener(onTransformed);
            pieceTextAnimFinished.RemoveListener(onAnimFinished);
            turnEnded.RemoveListener(onTurnEnded);
            pieceDied.RemoveListener(onPieceDied);

            startTarget.RemoveListener(onStartSelectTarget);
            targetSelected.RemoveListener(onTargetSelected);
            targetCancel.RemoveListener(onTargetCancel);

            startAbilityTarget.RemoveListener(onStartSelectAbilityTarget);
            targetAbilitySelected.RemoveListener(onTargetAbilitySelected);
            targetAbilityCancel.RemoveListener(onTargetAbilityCancel);
        }

        public void onMove(PieceMovedModel pieceMoved)
        {
            checkEnemiesInRange();
            if (pieceMoved.piece != view.piece) return;

            animationQueue.Add(
                new PieceView.RotateAnim()
                {
                    piece = view,
                    destAngle = DirectionAngle.angle[pieceMoved.direction]
                }
            );

            animationQueue.Add(
                new PieceView.MoveAnim()
                {
                    piece = view.piece,
                    destination = pieceMoved.to.gameObject.transform.position,
                    finishedMoving = pieceFinishedMoving,
                    isTeleport = pieceMoved.isTeleport
                }
            );
        }

        public void onAttacked(AttackPieceModel attackPiece)
        {
            if(attackPiece.attackingPieceId != view.piece.id) return;

            //TODO: Add more animation
            animationQueue.Add(
                new PieceView.RotateAnim()
                {
                    piece = view,
                    destAngle = DirectionAngle.angle[attackPiece.direction]
                }
            );
        }

        public void onRotated(RotatePieceModel rotatePiece)
        {
            if(rotatePiece.pieceId != view.piece.id) return;

            //TODO: Add more animation
            animationQueue.Add(
                new PieceView.RotateAnim()
                {
                    piece = view,
                    destAngle = DirectionAngle.angle[rotatePiece.direction]
                }
            );
        }

        public void onHealthChange(PieceHealthChangeModel hpChange)
        {
            if (hpChange.pieceId != view.piece.id) return;

            view.piece.health = hpChange.newCurrentHealth;

            animationQueue.Add(
                new PieceView.TakeDamageAnim()
                {
                    text = view.damageSplatText,
                    bonusText = view.damageSplatBonusText,
                    bonus = hpChange.bonus,
                    bonusMsg = hpChange.bonusMsg,
                    damageSplat = view.damageSplat,
                    damageTaken = hpChange.change
                }
            );

            animationQueue.Add(
                new PieceView.UpdateTextAnim()
                {
                    text = view.healthText,
                    textGO = view.healthGO,
                    current = view.piece.health,
                    original = view.piece.baseHealth,
                    change = hpChange.change,
                    animFinished = pieceTextAnimFinished,
                    piece = view.piece
                }
            );

            if (hpChange.armorChange != 0)
            {
                animationQueue.Add(
                    new PieceView.UpdateArmorAnim()
                    {
                        text = view.armorText,
                        textGO = view.armorGO,
                        textBG = view.armorBG,
                        current = view.piece.armor,
                        change = hpChange.armorChange,
                        piece = view.piece
                    }
                );
            }
        }

        public void onArmorChange(PieceArmorChangeModel armorChange)
        {
            if(armorChange.pieceId != view.piece.id) return;

            if (armorChange.change != 0)
            {
                animationQueue.Add(
                    new PieceView.UpdateArmorAnim()
                    {
                        text = view.armorText,
                        textGO = view.armorGO,
                        textBG = view.armorBG,
                        current = view.piece.armor,
                        change = armorChange.change,
                        piece = view.piece
                    }
                );
            }
        }

        public void onAttrChange(PieceAttributeChangeModel attrChange)
        {
            if(attrChange.pieceId != view.piece.id) return;
            //we don't actually know change in this case, but pass in -1 to always show
            //the punch size
            if (attrChange.attack != null || attrChange.baseAttack != null)
            {
                animationQueue.Add(
                    new PieceView.UpdateTextAnim()
                    {
                        text = view.attackText,
                        textGO = view.attackGO,
                        current = view.piece.attack,
                        original = view.piece.baseAttack,
                        change = -1, 
                        animFinished = pieceTextAnimFinished,
                        piece = view.piece
                    }
                );
            }

            if (attrChange.health != null || attrChange.baseHealth != null)
            {
                animationQueue.Add(
                    new PieceView.UpdateTextAnim()
                    {
                        text = view.healthText,
                        textGO = view.healthGO,
                        current = view.piece.health,
                        original = view.piece.baseHealth,
                        change = -1, 
                        animFinished = pieceTextAnimFinished,
                        piece = view.piece
                    }
                );
            }
        }

        public void onBuffed(PieceBuffModel pieceBuff)
        {
            if(pieceBuff.pieceId != view.piece.id) return;

            if (pieceBuff.attack != null)
            {
                animationQueue.Add(
                    new PieceView.UpdateTextAnim()
                    {
                        text = view.attackText,
                        textGO = view.attackGO,
                        current = view.piece.attack,
                        original = view.piece.baseAttack,
                        change = pieceBuff.attack.Value, 
                        animFinished = pieceTextAnimFinished,
                        piece = view.piece
                    }
                );
            }

            if (pieceBuff.health != null)
            {
                animationQueue.Add(
                    new PieceView.UpdateTextAnim()
                    {
                        text = view.healthText,
                        textGO = view.healthGO,
                        current = view.piece.health,
                        original = view.piece.baseHealth,
                        change = pieceBuff.health.Value, 
                        animFinished = pieceTextAnimFinished,
                        piece = view.piece
                    }
                );
            }
        }

        public void onStatusChange(PieceStatusChangeModel pieceStatusChange)
        {
            if(pieceStatusChange.pieceId != view.piece.id) return;


            //Animate?
            view.circleBg.SetActive(false);
            view.deathIcon.SetActive(false);

            //as above, we don't actually know change in this case, but pass in -1 to always show
            //the punch size
            if (pieceStatusChange.newAttack != null)
            {
                animationQueue.Add(
                    new PieceView.UpdateTextAnim()
                    {
                        text = view.attackText,
                        textGO = view.attackGO,
                        current = view.piece.attack,
                        original = view.piece.baseAttack,
                        change = -1, 
                        animFinished = pieceTextAnimFinished,
                        piece = view.piece
                    }
                );
            }

            if (pieceStatusChange.newHealth != null)
            {
                animationQueue.Add(
                    new PieceView.UpdateTextAnim()
                    {
                        text = view.healthText,
                        textGO = view.healthGO,
                        current = view.piece.health,
                        original = view.piece.baseHealth,
                        change = -1, 
                        animFinished = pieceTextAnimFinished,
                        piece = view.piece
                    }
                );
            }
        }

        public void onTransformed(TransformPieceModel transformed)
        {
            if(transformed.pieceId != view.piece.id) return;

            animationQueue.Add(
                new PieceView.UpdateTextAnim()
                {
                    text = view.attackText,
                    textGO = view.attackGO,
                    current = view.piece.attack,
                    original = view.piece.baseAttack,
                    change = -1, 
                    animFinished = pieceTextAnimFinished,
                    piece = view.piece
                }
            );
            animationQueue.Add(
                new PieceView.UpdateTextAnim()
                {
                    text = view.healthText,
                    textGO = view.healthGO,
                    current = view.piece.health,
                    original = view.piece.baseHealth,
                    change = -1, 
                    animFinished = pieceTextAnimFinished,
                    piece = view.piece
                }
            );
        }

        private void onAnimFinished(PieceModel pieceModel)
        {
            if(pieceModel != view.piece) return;

            if (pieceModel.health <= 0)
            {
                animationQueue.Add(
                    new PieceView.DieAnim()
                    {
                        piece = pieceModel,
                        pieceDied = pieceDied
                    }
                );
            }
        }

        private void onStartSelectTarget(TargetModel model)
        {
            if (model.targets != null && model.targets.targetPieceIds.Contains(view.piece.id))
            {
                view.targetCandidate = true;
            }
        }
        private void onTargetSelected(TargetModel c)
        {
            view.targetCandidate = false;
        }
        private void onTargetCancel(CardModel card)
        {
            view.targetCandidate = false;
        }

        private void onStartSelectAbilityTarget(StartAbilityTargetModel model)
        {
            if (model.targets.targetPieceIds.Contains(view.piece.id))
            {
                view.targetCandidate = true;
            }
        }
        private void onTargetAbilitySelected(StartAbilityTargetModel c, PieceModel m)
        {
            view.targetCandidate = false;
        }
        private void onTargetAbilityCancel(PieceModel card)
        {
            view.targetCandidate = false;
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            view.UpdateTurn(turns.currentPlayerId);
        }

        private void onPieceDied(PieceModel p)
        {
            checkEnemiesInRange();
        }

        private void checkEnemiesInRange()
        {
            var pieceLocation = view.piece.tilePosition;
            var neighbors = mapService.GetNeighbors(pieceLocation);
            neighbors = neighbors.Where(t => mapService.isHeightPassable(t.Value, mapService.Tile(pieceLocation)))
                .ToDictionary(k => k.Key, v => v.Value);

            view.enemiesInRange = pieces.Pieces.Any(p => 
                p.playerId != view.piece.playerId
                && neighbors.ContainsKey(p.tilePosition)
            );
        }
    }
}


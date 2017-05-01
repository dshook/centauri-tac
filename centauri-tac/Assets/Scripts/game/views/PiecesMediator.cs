using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class PiecesMediator : Mediator
    {
        [Inject] public PieceMovedSignal pieceMoved { get; set; }
        [Inject] public PieceAttackedSignal pieceAttacked { get; set; }
        [Inject] public PieceRotatedSignal pieceRotated { get; set; }

        [Inject] public PieceHealthChangedSignal pieceHpChanged { get; set; }
        [Inject] public PieceAttributeChangedSignal pieceAttrChanged { get; set; }
        [Inject] public PieceBuffSignal pieceBuffed { get; set; }
        [Inject] public PieceStatusChangeSignal pieceStatusChanged { get; set; }
        [Inject] public PieceTransformedSignal pieceTransformed { get; set; }
        [Inject] public PieceArmorChangedSignal pieceArmorChanged { get; set; }
        [Inject] public PieceCharmedSignal pieceCharmed { get; set; }
        [Inject] public GameFinishedSignal gameFinished { get; set; }

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

        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public IMapService mapService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

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
            pieceCharmed.AddListener(onCharmed);
            gameFinished.AddListener(onGameFinished);

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
            pieceCharmed.RemoveListener(onCharmed);
            gameFinished.RemoveListener(onGameFinished);

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
            var view = pieceMoved.piece.pieceView;

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
                    anim = view.anim,
                    destination = pieceMoved.to.gameObject.transform.position,
                    finishedMoving = pieceFinishedMoving,
                    isTeleport = pieceMoved.isTeleport
                }
            );
        }

        public void onAttacked(AttackPieceModel attackPiece)
        {
            var attacker = pieces.Piece(attackPiece.attackingPieceId);
            if(attacker == null) return;

            //TODO: Add more animation
            animationQueue.Add(
                new PieceView.RotateAnim()
                {
                    piece = attacker.pieceView,
                    destAngle = DirectionAngle.angle[attackPiece.direction]
                }
            );
            animationQueue.Add(
                new PieceView.EventTriggerAnim()
                {
                    piece = attacker.pieceView,
                    eventName = "onAttack"
                }
            );

            var target = pieces.Piece(attackPiece.targetPieceId);
            if (target != null)
            {
                animationQueue.Add(
                    new PieceView.RotateAnim()
                    {
                        piece = target.pieceView,
                        destAngle = DirectionAngle.angle[attackPiece.targetDirection]
                    }
                );
            }
        }

        public void onRotated(RotatePieceModel rotatePiece)
        {
            var piece = pieces.Piece(rotatePiece.pieceId);
            if(piece == null) return;

            animationQueue.Add(
                new PieceView.RotateAnim()
                {
                    piece = piece.pieceView,
                    destAngle = DirectionAngle.angle[rotatePiece.direction]
                }
            );
        }

        public void onCharmed(CharmPieceModel charm)
        {
            var piece = pieces.Piece(charm.pieceId);
            if(piece == null) return;

            animationQueue.Add(
                new PieceView.UpdateHpBarAnim()
                {
                    piece = piece.pieceView
                }
            );
        }

        public void onHealthChange(PieceHealthChangeModel hpChange)
        {
            var piece = pieces.Piece(hpChange.pieceId);
            if(piece == null) return;

            var view = piece.pieceView;
            view.piece.health = hpChange.newCurrentHealth;

            if (hpChange.change < 0)
            {
                animationQueue.Add(
                    new PieceView.EventTriggerAnim()
                    {
                        piece = view,
                        eventName = "onHit"
                    }
                );
            }

            var numberSplat = loader.Load<GameObject>("NumberSplat");
            animationQueue.Add(
                new PieceView.TakeDamageAnim()
                {
                    parent = piece.pieceView.faceCameraContainer.transform,
                    bonus = hpChange.bonus,
                    bonusMsg = hpChange.bonusMsg,
                    numberSplat = numberSplat,
                    change = hpChange.change
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

            animationQueue.Add(
                new PieceView.UpdateHpBarAnim()
                {
                    piece = view
                }
            );
        }

        public void onArmorChange(PieceArmorChangeModel armorChange)
        {
            var piece = pieces.Piece(armorChange.pieceId);
            if(piece == null) return;

            var view = piece.pieceView;

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
            var piece = pieces.Piece(attrChange.pieceId);
            if(piece == null) return;

            var view = piece.pieceView;

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

            animationQueue.Add(
                new PieceView.UpdateHpBarAnim()
                {
                    piece = view
                }
            );
        }

        public void onBuffed(PieceBuffModel pieceBuff)
        {
            var piece = pieces.Piece(pieceBuff.pieceId);
            if(piece == null) return;

            var view = piece.pieceView;

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

            animationQueue.Add(
                new PieceView.UpdateHpBarAnim()
                {
                    piece = view
                }
            );
        }

        public void onStatusChange(PieceStatusChangeModel pieceStatusChange)
        {
            var piece = pieces.Piece(pieceStatusChange.pieceId);
            if(piece == null) return;

            var view = piece.pieceView;

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

            animationQueue.Add(
                new PieceView.UpdateHpBarAnim()
                {
                    piece = view
                }
            );
        }

        public void onTransformed(TransformPieceModel transformed)
        {
            var piece = pieces.Piece(transformed.pieceId);
            if(piece == null) return;

            var view = piece.pieceView;

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

            animationQueue.Add(
                new PieceView.UpdateHpBarAnim()
                {
                    piece = view
                }
            );
        }

        private void onAnimFinished(PieceModel pieceModel)
        {
            if (pieceModel.health <= 0)
            {
                animationQueue.Add(
                    new PieceView.DieAnim()
                    {
                        piece = pieceModel,
                        anim = pieceModel.pieceView.anim,
                        pieceDied = pieceDied,
                        isExact = pieceModel.health == 0,
                        isBig = pieceModel.health <= -5
                    }
                );
            }
        }

        private void onStartSelectTarget(TargetModel model)
        {
            if (model.targets != null )
            {
                var selected = pieces.Pieces.Where(p => model.targets.targetPieceIds.Contains(p.id));
                foreach (var piece in selected)
                {
                    piece.pieceView.targetCandidate = true;
                }
            }
        }
        private void onTargetSelected(TargetModel c)
        {
            disableTargetCandidate();
        }
        private void onTargetCancel(CardModel card)
        {
            foreach (var piece in pieces.Pieces)
            {
                piece.pieceView.targetCandidate = false;
            }
        }

        private void onStartSelectAbilityTarget(StartAbilityTargetModel model)
        {
            var selected = pieces.Pieces.Where(p => model.targets.targetPieceIds.Contains(p.id));
            foreach (var piece in selected)
            {
                piece.pieceView.targetCandidate = true;
            }
        }
        private void onTargetAbilitySelected(StartAbilityTargetModel c, PieceModel m)
        {
            disableTargetCandidate();
        }
        private void onTargetAbilityCancel(PieceModel card)
        {
            disableTargetCandidate();
        }

        private void disableTargetCandidate()
        {
            foreach (var piece in pieces.Pieces)
            {
                piece.pieceView.targetCandidate = false;
            }
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            if(!turns.isClientSwitch) return;

            var meId = players.Me.id;
            var opponentId = players.Opponent(meId).id;
            foreach (var piece in pieces.Pieces)
            {
                piece.currentPlayerHasControl = piece.playerId == meId;
                piece.pieceView.UpdateTurn();
            }
        }

        private void onPieceDied(PieceModel p)
        {
            checkEnemiesInRange();
        }

        private void onGameFinished(GameFinishedModel gf)
        {
            foreach (var piece in pieces.Pieces)
            {
                string eventName = piece.playerId == gf.winnerId ? "onWin" : "onLose";

                animationQueue.Add(
                    new PieceView.EventTriggerAnim()
                    {
                        piece = piece.pieceView,
                        eventName = eventName
                    }
                );
            }
        }

        private void checkEnemiesInRange()
        {
            foreach (var piece in pieces.Pieces)
            {
                var view = piece.pieceView;

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
}


using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class PiecesMediator : Mediator
    {
        [Inject] public PieceTextAnimationFinishedSignal pieceTextAnimFinished { get; set; }
        [Inject] public PieceFinishedMovingSignal pieceFinishedMoving { get; set; }
        [Inject] public PieceDiedSignal pieceDied { get; set; }

        [Inject] public AnimationQueueModel animationQueue { get; set; }

        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public MapModel map { get; set; }

        [Inject] public IMapService mapService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        [ListensTo(typeof(PieceSpawnedSignal))]
        public void onPieceSpawn(PieceSpawnedModel piece)
        {
            if (piece.alreadyDeployed) return;

            animationQueue.Add(new PieceView.SpawnAnim() {
                piece = piece.piece.pieceView,
                map = map,
                mapService = mapService,
                loader = loader,
                Async = piece.runAsync
            });
            animationQueue.Add(
                new PieceView.ChangeStatusAnim()
                {
                    pieceView = piece.piece.pieceView,
                    pieceStatusChange = new PieceStatusChangeModel() { add = piece.piece.statuses, statuses = piece.piece.statuses }
                }
            );
        }

        [ListensTo(typeof(PieceMovedSignal))]
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

        [ListensTo(typeof(PieceAttackedSignal))]
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

        [ListensTo(typeof(PieceRotatedSignal))]
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

        [ListensTo(typeof(PieceCharmedSignal))]
        public void onCharmed(CharmPieceModel charm)
        {
            var piece = pieces.Piece(charm.pieceId);
            if(piece == null) return;

            animationQueue.Add(
                new PieceView.CharmAnim()
                {
                    piece = piece.pieceView,
                    loader = loader
                }
            );
        }

        [ListensTo(typeof(PieceHealthChangedSignal))]
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

        [ListensTo(typeof(PieceArmorChangedSignal))]
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

        [ListensTo(typeof(PieceAttributeChangedSignal))]
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

        [ListensTo(typeof(PieceBuffSignal))]
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

        [ListensTo(typeof(PieceStatusChangeSignal))]
        public void onStatusChange(PieceStatusChangeModel pieceStatusChange)
        {
            var piece = pieces.Piece(pieceStatusChange.pieceId);
            if(piece == null) return;

            var view = piece.pieceView;

            animationQueue.Add(
                new PieceView.ChangeStatusAnim()
                {
                    pieceView = view,
                    pieceStatusChange = pieceStatusChange
                }
            );

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

        [ListensTo(typeof(PieceTransformedSignal))]
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

            //figure out added and removed statuses based on previous and current status states
            Statuses add = piece.statuses & (piece.statuses ^ transformed.oldStatuses);
            Statuses remove = transformed.oldStatuses & (piece.statuses ^ transformed.oldStatuses);

            animationQueue.Add(
                new PieceView.ChangeStatusAnim()
                {
                    pieceView = view,
                    pieceStatusChange = new PieceStatusChangeModel() {
                        add = add,
                        remove = remove,
                        statuses = piece.statuses
                    }
                }
            );
        }

        [ListensTo(typeof(PieceTextAnimationFinishedSignal))]
        public void onAnimFinished(PieceModel pieceModel)
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

        [ListensTo(typeof(StartSelectTargetSignal))]
        public void onStartSelectTarget(TargetModel model)
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
        [ListensTo(typeof(SelectTargetSignal))]
        public void onTargetSelected(TargetModel c)
        {
            disableTargetCandidate();
        }
        [ListensTo(typeof(CancelSelectTargetSignal))]
        public void onTargetCancel(CardModel card)
        {
            foreach (var piece in pieces.Pieces)
            {
                piece.pieceView.targetCandidate = false;
            }
        }

        [ListensTo(typeof(StartSelectAbilityTargetSignal))]
        public void onStartSelectAbilityTarget(StartAbilityTargetModel model)
        {
            var selected = pieces.Pieces.Where(p => model.targets.targetPieceIds.Contains(p.id));
            foreach (var piece in selected)
            {
                piece.pieceView.targetCandidate = true;
            }
        }
        [ListensTo(typeof(SelectAbilityTargetSignal))]
        public void onTargetAbilitySelected(StartAbilityTargetModel c, PieceModel m)
        {
            disableTargetCandidate();
        }
        [ListensTo(typeof(CancelSelectAbilityTargetSignal))]
        public void onTargetAbilityCancel(PieceModel card)
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

        [ListensTo(typeof(TurnEndedSignal))]
        public void onTurnEnded(GameTurnModel turns)
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

        [ListensTo(typeof(PieceDiedSignal))]
        public void onPieceDied(PieceModel p)
        {
            checkEnemiesInRange();
        }

        [ListensTo(typeof(GameFinishedSignal))]
        public void onGameFinished(GameFinishedModel gf)
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


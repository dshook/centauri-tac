using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
                    piece = piece.piece.pieceView,
                    loader = loader,
                    pieceStatusChange = new PieceStatusChangeModel() { add = piece.piece.statuses, statuses = piece.piece.statuses }
                }
            );
            checkEnemiesInRange();
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
                    piece = view,
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
                    piece = view
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
                        piece = view
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
                        piece = view
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
                        piece = view
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
                        piece = view
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
                        piece = view
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
                        piece = view
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
                    piece = view,
                    loader = loader,
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
                        piece = view
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
                        piece = view
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
                    piece = view
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
                    piece = view
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
                    piece = view,
                    loader = loader,
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
                        piece = pieceModel.pieceView,
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
            checkEnemiesInRange();

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
            pieces.Pieces.Remove(p);
            checkEnemiesInRange();
            StartCoroutine(CleanupPiece(p));
        }


        private IEnumerator CleanupPiece(PieceModel pieceDied)
        {
            while(true){
                var remainingAnimations = animationQueue.PieceHasAnimation(pieceDied.pieceView);
                if(!remainingAnimations){
                    GameObject.Destroy(pieceDied.gameObject, 0.1f);
                    break;
                }
                yield return new WaitForSeconds(0.08f);
            }
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

                Dictionary<Vector2, Tile> attackRangeTiles = null;
                if (piece.isRanged)
                {
                    attackRangeTiles = mapService.GetKingTilesInRadius(piece.tilePosition, piece.range.Value);
                }else{
                    //find where the piece can move regardless of enemies, with a bonus movement of 1 so it includes
                    //spots where enemies are
                    attackRangeTiles = mapService.GetMovementTilesInRadius(piece, false, true, 1);
                }

                view.attackRangeTiles = attackRangeTiles == null ? null : attackRangeTiles
                    .Where(t => piece.canAttackTile(pieces, t.Value))
                    .Select(t => t.Value)
                    .ToList();

                view.enemiesInRange = attackRangeTiles == null ? false : attackRangeTiles.Any(t => {
                    var occupyingPiece = pieces.Pieces.FirstOrDefault(m => m.tilePosition == t.Key);
                    return occupyingPiece != null && piece.canAttackTile(pieces, t.Value);
                });
            }
        }
    }
}


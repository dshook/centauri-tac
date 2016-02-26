using strange.extensions.mediation.impl;
using ctac.signals;
using UnityEngine;

namespace ctac
{
    public class PieceMediator : Mediator
    {
        [Inject] public PieceView view { get; set; }

        [Inject] public PieceMovedSignal pieceMoved { get; set; }
        [Inject] public PieceAttackedSignal pieceAttacked { get; set; }

        [Inject] public PieceHealthChangedSignal pieceHpChanged { get; set; }
        [Inject] public PieceAttributeChangedSignal pieceAttrChanged { get; set; }
        [Inject] public PieceBuffSignal pieceBuffed { get; set; }

        [Inject] public PieceTextAnimationFinishedSignal pieceTextAnimFinished { get; set; }
        [Inject] public PieceFinishedMovingSignal pieceFinishedMoving { get; set; }

        [Inject] public StartSelectTargetSignal startTarget { get; set; }
        [Inject] public SelectTargetSignal targetSelected { get; set; }
        [Inject] public CancelSelectTargetSignal targetCancel { get; set; }

        [Inject] public PieceDiedSignal pieceDied { get; set; }
        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public AnimationQueueModel animationQueue { get; set; }

        public override void OnRegister()
        {
            pieceMoved.AddListener(onMove);
            pieceAttacked.AddListener(onAttacked);
            pieceHpChanged.AddListener(onHealthChange);
            pieceAttrChanged.AddListener(onAttrChange);
            pieceBuffed.AddListener(onBuffed);
            pieceTextAnimFinished.AddListener(onAnimFinished);
            startTarget.AddListener(onStartSelectTarget);
            targetSelected.AddListener(onTargetSelected);
            targetCancel.AddListener(onTargetCancel);
            turnEnded.AddListener(onTurnEnded);
        }

        public override void onRemove()
        {
            pieceMoved.RemoveListener(onMove);
            pieceAttacked.RemoveListener(onAttacked);
            pieceHpChanged.RemoveListener(onHealthChange);
            pieceAttrChanged.RemoveListener(onAttrChange);
            pieceBuffed.RemoveListener(onBuffed);
            pieceTextAnimFinished.RemoveListener(onAnimFinished);
            startTarget.RemoveListener(onStartSelectTarget);
            targetSelected.RemoveListener(onTargetSelected);
            targetCancel.RemoveListener(onTargetCancel);
            turnEnded.RemoveListener(onTurnEnded);
        }

        public void onMove(PieceMovedModel pieceMoved)
        {
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
                    finishedMoving = pieceFinishedMoving
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

        public void onHealthChange(PieceHealthChangeModel hpChange)
        {
            if (hpChange.pieceId != view.piece.id) return;

            view.piece.health = hpChange.newCurrentHealth;

            if (hpChange.change < 0)
            {
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
            }

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

            //TODO: display movement
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

            //TODO: display movement
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

        private void onStartSelectTarget(StartTargetModel model)
        {
            if (model.targets.targetPieceIds.Contains(view.piece.id))
            {
                view.targetCandidate = true;
            }
        }
        private void onTargetSelected(StartTargetModel c, PieceModel m)
        {
            view.targetCandidate = false;
        }
        private void onTargetCancel(CardModel card)
        {
            view.targetCandidate = false;
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            view.UpdateTurn(turns.currentPlayerId);
        }
    }
}


using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PieceMediator : Mediator
    {
        [Inject]
        public PieceView view { get; set; }

        [Inject]
        public PieceMovedSignal pieceMoved { get; set; }

        [Inject]
        public PieceAttackedSignal pieceAttacked { get; set; }

        [Inject]
        public PieceHealthChangedSignal pieceHpChanged { get; set; }

        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        [Inject]
        public PieceAttackedAnimationSignal pieceAttackedAnim { get; set; }

        [Inject]
        public PieceFinishedMovingSignal pieceFinishedMoving { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        public override void OnRegister()
        {
            pieceMoved.AddListener(onMove);
            pieceAttacked.AddListener(onAttacked);
            pieceHpChanged.AddListener(onHealthChange);
            pieceAttackedAnim.AddListener(onAttackFinished);
        }

        public override void onRemove()
        {
            pieceMoved.RemoveListener(onMove);
            pieceAttacked.RemoveListener(onAttacked);
            pieceHpChanged.RemoveListener(onHealthChange);
            pieceAttackedAnim.RemoveListener(onAttackFinished);
        }

        public void onMove(PieceMovedModel pieceMoved)
        {
            if (pieceMoved.piece != view.piece) return;

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
            //TODO: Add moving animation
        }

        public void onHealthChange(PieceHealthChangeModel hpChange)
        {
            if (hpChange.pieceId == view.piece.id)
            {
                view.piece.health = hpChange.newCurrentHealth;

                if (hpChange.change < 0)
                {
                    animationQueue.Add(
                        new PieceView.TakeDamageAnim()
                        {
                            text = view.damageSplatText,
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
                        attackFinished = pieceAttackedAnim,
                        piece = view.piece
                    }
                );
            }
        }

        private void onAttackFinished(PieceModel pieceModel)
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

    }
}


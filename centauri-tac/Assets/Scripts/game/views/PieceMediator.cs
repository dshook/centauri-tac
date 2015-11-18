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
            pieceAttackedAnim.AddListener(onAttackFinished);
        }

        public override void onRemove()
        {
            pieceMoved.RemoveListener(onMove);
            pieceAttacked.RemoveListener(onAttacked);
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
            int change = 0;
            if (attackPiece.attackingPieceId == view.piece.id)
            {
                change = attackPiece.attackerNewHp - view.piece.health;
                view.piece.health = attackPiece.attackerNewHp;
            }
            else if (attackPiece.targetPieceId == view.piece.id)
            {
                change = attackPiece.targetNewHp - view.piece.health;
                view.piece.health = attackPiece.targetNewHp;
            }

            if (attackPiece.attackingPieceId == view.piece.id || attackPiece.targetPieceId == view.piece.id)
            {
                animationQueue.Add(
                    new PieceView.UpdateTextAnim()
                    {
                        text = view.healthText,
                        textGO = view.healthGO,
                        current = view.piece.health,
                        original = view.piece.baseHealth,
                        change = change,
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


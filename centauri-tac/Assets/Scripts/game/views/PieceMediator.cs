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

        public void onMove(PieceModel pieceMoved, Tile dest)
        {
            if (pieceMoved != view.piece) return;

            animationQueue.Add(
                new PieceView.MoveAnim()
                {
                    piece = view.piece.gameObject,
                    destination = dest.gameObject.transform.position
                }
            );
        }

        public void onAttacked(AttackPieceModel attackPiece)
        {
            if (attackPiece.attackingPieceId == view.piece.id)
            {
                view.piece.health = attackPiece.attackerNewHp;
            }
            else if (attackPiece.targetPieceId == view.piece.id)
            {
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
                        original = view.piece.originalHealth,
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


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
        public PieceAttributeChangedSignal pieceAttrChanged { get; set; }

        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        [Inject]
        public PieceTextAnimationFinishedSignal pieceTextAnimFinished { get; set; }

        [Inject]
        public PieceFinishedMovingSignal pieceFinishedMoving { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        public override void OnRegister()
        {
            pieceMoved.AddListener(onMove);
            pieceAttacked.AddListener(onAttacked);
            pieceHpChanged.AddListener(onHealthChange);
            pieceAttrChanged.AddListener(onAttrChange);
            pieceTextAnimFinished.AddListener(onAnimFinished);
        }

        public override void onRemove()
        {
            pieceMoved.RemoveListener(onMove);
            pieceAttacked.RemoveListener(onAttacked);
            pieceHpChanged.RemoveListener(onHealthChange);
            pieceAttrChanged.RemoveListener(onAttrChange);
            pieceTextAnimFinished.RemoveListener(onAnimFinished);
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
            if (hpChange.pieceId != view.piece.id) return;

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

    }
}


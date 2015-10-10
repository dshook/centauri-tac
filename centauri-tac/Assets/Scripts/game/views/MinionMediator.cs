using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class MinionMediator : Mediator
    {
        [Inject]
        public MinionView view { get; set; }

        [Inject]
        public MinionMovedSignal minionMoved { get; set; }

        [Inject]
        public MinionAttackedSignal minionAttacked { get; set; }

        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        public override void OnRegister()
        {
            minionMoved.AddListener(onMove);
            minionAttacked.AddListener(onAttacked);
        }

        public override void onRemove()
        {
            minionMoved.RemoveListener(onMove);
            minionAttacked.RemoveListener(onAttacked);
        }

        public void onMove(MinionModel minionMoved, Tile dest)
        {
            if (minionMoved != view.minion) return;

            animationQueue.Add(
                new MinionView.MoveAnim()
                {
                    minion = view.minion.gameObject,
                    destination = dest.gameObject.transform.position
                }
            );
        }

        public void onAttacked(AttackPieceModel attackPiece)
        {
            if (attackPiece.attackingPieceId == view.minion.id)
            {
                view.minion.health = attackPiece.attackerNewHp;
            }
            else if (attackPiece.targetPieceId == view.minion.id)
            {
                view.minion.health = attackPiece.targetNewHp;
            }

            animationQueue.Add(
                new MinionView.UpdateTextAnim()
                {
                    text = view.healthText,
                    textGO = view.healthGO,
                    current = view.minion.health,
                    original = view.minion.originalHealth
                }
            );

            if (view.minion.health <= 0)
            {
                animationQueue.Add(
                    new MinionView.DieAnim()
                    {
                        minion = view.minion.gameObject
                    }
                );
            }
        }

    }
}


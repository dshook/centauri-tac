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

            view.AddToPath(dest);
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

            if (view.minion.health < 0)
            {
                Destroy(view.minion.gameObject);
            }
        }

    }
}


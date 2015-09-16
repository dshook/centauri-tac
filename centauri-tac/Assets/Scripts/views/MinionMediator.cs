using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class MinionMediator : Mediator
    {
        [Inject]
        public MinionView view { get; set; }

        [Inject]
        public MinionMoveSignal minionMove { get; set; }

        public override void OnRegister()
        {
            minionMove.AddListener(onMinionMove);
        }

        public override void onRemove()
        {
            minionMove.RemoveListener(onMinionMove);
        }

        public void onMinionMove(IMinionModel minionMoved, Tile dest)
        {
            if (minionMoved != view.minion) return;

            view.Move(dest);
        }

    }
}


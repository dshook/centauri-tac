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

        [Inject]
        public MapModel map { get; set; }

        public override void OnRegister()
        {
            minionMove.AddListener(onMinionMove);
        }

        public override void onRemove()
        {
            minionMove.RemoveListener(onMinionMove);
        }

        public void onMinionMove(MinionModel minionMoved, Tile dest)
        {
            if (minionMoved != view.minion) return;

            var startTile = map.tiles.Get(minionMoved.tilePosition);
            var path = map.FindPath(startTile, dest);

            view.MovePath(path);
            minionMoved.hasMoved = true;
        }

    }
}


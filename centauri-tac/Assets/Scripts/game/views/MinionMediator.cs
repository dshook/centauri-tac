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
        public MoveMinionSignal moveMinion { get; set; }

        [Inject]
        public MinionMovedSignal minionMove { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public IMapService mapService { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        public override void OnRegister()
        {
            moveMinion.AddListener(onRequestMove);
            minionMove.AddListener(onMove);
        }

        public override void onRemove()
        {
            moveMinion.RemoveListener(onRequestMove);
            minionMove.RemoveListener(onMove);
        }

        public void onRequestMove(MinionModel minionMoved, Tile dest)
        {
            if (minionMoved != view.minion) return;

            var startTile = map.tiles.Get(minionMoved.tilePosition);
            var path = mapService.FindPath(startTile, dest, minionMoved.moveDist);
            //format for server
            var serverPath = path.Select(x => new PositionModel(x.position) ).ToList();
            socket.Request(gameTurn.currentTurnClientId, "game", "move", new { pieceId = view.minion.id, route = serverPath });

            minionMoved.hasMoved = true;
        }

        public void onMove(MinionModel minionMoved, Tile dest)
        {
            if (minionMoved != view.minion) return;

            view.AddToPath(dest);
        }

    }
}


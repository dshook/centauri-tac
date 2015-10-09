using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class MovePieceCommand : Command
    {
        [Inject]
        public MinionModel minionMoved { get; set; }

        [Inject]
        public Tile dest { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public IMapService mapService { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }


        public override void Execute()
        {

            var startTile = map.tiles.Get(minionMoved.tilePosition);
            var path = mapService.FindPath(startTile, dest, minionMoved.moveDist);
            //format for server
            var serverPath = path.Select(x => new PositionModel(x.position) ).ToList();
            socket.Request(gameTurn.currentTurnClientId, "game", "move", new { pieceId = minionMoved.id, route = serverPath });

            minionMoved.hasMoved = true;
        }
    }
}


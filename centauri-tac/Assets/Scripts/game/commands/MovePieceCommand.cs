using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class MovePieceCommand : Command
    {
        [Inject]
        public PieceModel pieceMoved { get; set; }

        [Inject]
        public Tile dest { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public IMapService mapService { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GamePlayersModel players { get; set; }


        public override void Execute()
        {

            var startTile = map.tiles.Get(pieceMoved.tilePosition);
            var path = mapService.FindPath(startTile, dest, pieceMoved.movement, players.Me.id);
            if (path == null || path.Count == 0) return;
            //format for server
            var serverPath = path.Select(x => new PositionModel(x.position) ).ToList();
            socket.Request(players.Me.clientId, "game", "move", new { pieceId = pieceMoved.id, route = serverPath });
        }
    }
}


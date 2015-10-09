using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;
using System.Collections.Generic;

namespace ctac
{
    public class AttackPieceCommand : Command
    {
        [Inject]
        public AttackPieceModel attackModel { get; set; }

        [Inject]
        public MinionsModel pieces { get; set; }

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
            //attack is a combination of move to target and then attack 
            var attacker = pieces.Minion(attackModel.attackingPieceId);
            var startTile = map.tiles.Get(attacker.tilePosition);
            var destTile = map.tiles.Get(pieces.Minion(attackModel.targetPieceId).tilePosition);
            var path = mapService.FindPath(startTile, destTile, attacker.moveDist);
            List<PositionModel> serverPath = null;
            if (path.Count > 1)
            {
                //slice off the last move tile since it'll be the enemy and then format for server
                serverPath = path.Take(path.Count - 1).Select(x => new PositionModel(x.position)).ToList();

                attacker.hasMoved = true;
            }

            socket.Request(gameTurn.currentTurnClientId, "game", "moveattack", 
                new { attackModel.attackingPieceId, attackModel.targetPieceId, route = serverPath }
            );
        }
    }
}


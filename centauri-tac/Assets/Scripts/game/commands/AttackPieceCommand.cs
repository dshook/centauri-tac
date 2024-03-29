using strange.extensions.command.impl;
using System.Linq;
using System.Collections.Generic;

namespace ctac
{
    public class AttackPieceCommand : Command
    {
        [Inject]
        public AttackPieceModel attackModel { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

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
            //attack is a combination of move to target and then attack 
            var attacker = pieces.Piece(attackModel.attackingPieceId);
            List<Tile> path = null;
            if (attacker.isMelee)
            {
                path = mapService.FindMovePath(attacker, pieces.Piece(attackModel.targetPieceId), null);
            }

            if (path == null && attacker.isMelee)
            {
                //noop if can't find a path to attack to 
                return;
            }
            List<PositionModel> serverPath = null;
            if (path != null && path.Count > 1)
            {
                //slice off the last move tile since it'll be the enemy and then format for server
                serverPath = path.Take(path.Count - 1).Select(x => new PositionModel(x.position)).ToList();

                attacker.moveCount += serverPath.Count;
            }

            socket.Request(players.Me.clientId, "game", "moveattack", 
                new { attackModel.attackingPieceId, attackModel.targetPieceId, route = serverPath }
            );
        }
    }
}


using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;
using System.Linq;
using System;
using strange.extensions.context.api;

namespace ctac
{
    public class MovePieceCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public MinionsModel minionsModel { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public MovePieceModel movePiece { get; set; }

        [Inject]
        public MinionMoveSignal minionMove { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            //check to see if this piece has already been moved by another player
            //wrong atm
            if (minionsModel.minions.Any(x => x.id == movePiece.pieceId))
            {
                return;
            }

            var minion = minionsModel.minions.FirstOrDefault(x => x.id == movePiece.pieceId);
            var tile = map.tiles[movePiece.to.Vector3.ToTileCoordinates()];

            minionMove.Dispatch(minion, tile);

            debug.Log( string.Format("Moved minion {0} to {1}", movePiece.pieceId, movePiece.to) , socketKey );
        }
    }
}


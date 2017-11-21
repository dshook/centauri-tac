using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;
using System;

namespace ctac
{
    public class ActionMovePieceCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public MovePieceModel movePiece { get; set; }

        [Inject]
        public PieceMovedSignal pieceMove { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject] public IMapService mapService { get; set; }
        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(movePiece.id)) return;

            var piece = piecesModel.Pieces.FirstOrDefault(x => x.id == movePiece.pieceId);
            var toTileCoords = movePiece.to.Vector3.ToTileCoordinates();
            var distance = mapService.TileDistance(toTileCoords, piece.tilePosition);
            var toTile = map.tiles[toTileCoords];
            piece.tilePosition = toTileCoords;

            if ((piece.statuses & Statuses.Flying) != 0)
            {
                piece.moveCount += distance;
            }
            if (movePiece.isJump)
            {
                piece.moveCount++;
            }
            else
            {
                //theoretically this should always be 1 in this case I think
                piece.moveCount += distance;
            }

            //ranged can't move and attack
            if (piece.isRanged)
            {
                piece.attackCount++;
            }

            pieceMove.Dispatch(new PieceMovedModel()
            {
                piece = piece,
                to = toTile,
                direction = movePiece.direction,
                isTeleport = movePiece.isTeleport
            });

            debug.Log(string.Format("Moved piece {0} to {1}", movePiece.pieceId, movePiece.to), socketKey);
        }
    }
}


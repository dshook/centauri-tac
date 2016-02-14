using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class TilePieceIndicatorMediator : Mediator
    {
        [Inject]
        public TilePieceIndicatorView view { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public PieceSpawnedSignal pieceSpawned { get; set; }

        [Inject]
        public PieceMovedSignal pieceMoved { get; set; }
        [Inject]
        public PieceFinishedMovingSignal pieceFinishedMoving { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        public override void OnRegister()
        {
            pieceSpawned.AddListener(onPieceSpawned);
            pieceMoved.AddListener(onPieceMoved);
            pieceFinishedMoving.AddListener(onPieceFinishedMoving);
            turnEnded.AddListener(resetTiles);
            pieceDied.AddListener(onPieceDied);
            view.init();
        }

        public override void onRemove()
        {
            pieceSpawned.RemoveListener(onPieceSpawned);
            pieceMoved.RemoveListener(onPieceMoved);
            pieceFinishedMoving.RemoveListener(onPieceFinishedMoving);
            turnEnded.RemoveListener(resetTiles);
            pieceDied.RemoveListener(onPieceDied);
        }

        private void onPieceSpawned(PieceModel piece)
        {
            if (piece.currentPlayerHasControl)
            {
                view.SetFriendly(map.tiles[piece.tilePosition]);
            }
            else
            {
                view.SetEnemy(map.tiles[piece.tilePosition]);
            }
        }

        private void onPieceMoved(PieceMovedModel pieceMoved)
        {
            view.ClearTile(map.tiles[pieceMoved.piece.tilePosition]);
        }

        private void onPieceFinishedMoving(PieceModel piece)
        {
            resetTiles(null);
        }

        private void resetTiles(GameTurnModel turns)
        {
            view.ClearTiles(map.tileList);

            view.SetFriendly(
                pieces.Pieces.Where(x => x.currentPlayerHasControl)
                .Select(x => map.tiles[x.tilePosition])
                .ToList()
            );

            view.SetEnemy(
                pieces.Pieces.Where(x => !x.currentPlayerHasControl)
                .Select(x => map.tiles[x.tilePosition])
                .ToList()
            );
        }

        private void onPieceDied(PieceModel piece)
        {
            view.ClearTile(map.tiles[piece.tilePosition]);
        }

    }
}


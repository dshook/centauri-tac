using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class TileHighlightMediator : Mediator
    {
        [Inject]
        public TileHighlightView view { get; set; }
        
        [Inject]
        public TileHoverSignal tileHover { get; set; }

        [Inject]
        public PieceSelectedSignal pieceSelected { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public IMapService mapService { get; set; }

        private PieceModel selectedPiece = null;

        public override void OnRegister()
        {
            view.tileHover.AddListener(onTileHover);
            pieceSelected.AddListener(onPieceSelected);
            view.init();
        }

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);
        }

        void onTileHover(GameObject newHoverTile)
        {
            Tile tile = null;
            if (newHoverTile != null)
            {
                tile = map.tiles.Get(newHoverTile.transform.position.ToTileCoordinates());
            }
            tileHover.Dispatch(tile);
            view.onTileHover(tile);

            if (selectedPiece != null && tile != null && !selectedPiece.hasMoved)
            {
                var gameTile = map.tiles.Get(selectedPiece.tilePosition);
                var path = mapService.FindPath(gameTile, tile, selectedPiece.movement);
                view.onTileMovePath(path);

                if (
                    pieces.Pieces.Any(m => m.tilePosition == tile.position && !m.currentPlayerHasControl)
                    && path != null
                )
                {
                    view.onAttackTile(tile);
                }
                else
                {
                    view.onAttackTile(null);
                }
            }
            else
            {
                view.onTileMovePath(null);
                view.onAttackTile(null);
            }
        }

        private void onPieceSelected(PieceModel selectedPiece)
        {
            if (selectedPiece != null && !selectedPiece.isMoving)
            {
                var gameTile = map.tiles.Get(selectedPiece.tilePosition);
                this.selectedPiece = selectedPiece;

                view.onTileSelected(gameTile);

                if (!selectedPiece.hasMoved)
                {
                    //find movement
                    var moveTiles = mapService.GetMovementTilesInRadius(gameTile.position, selectedPiece.movement);
                    //take out the central one
                    moveTiles.Remove(gameTile.position);
                    view.onMovableTiles(moveTiles);
                }
            }
            else
            {
                this.selectedPiece = null;
                view.onTileSelected(null);
                view.onMovableTiles(null);
            }
        }
    }
}


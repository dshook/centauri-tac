using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class TileHighlightMediator : Mediator
    {
        [Inject]
        public TileHighlightView view { get; set; }
        
        [Inject]
        public TileHoverSignal tileHover { get; set; }

        [Inject]
        public MinionSelectedSignal minionSelected { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public IMapService mapService { get; set; }

        private MinionModel selectedMinion = null;

        public override void OnRegister()
        {
            view.tileHover.AddListener(onTileHover);
            minionSelected.AddListener(onMinionSelected);
            view.init();
        }

        public override void onRemove()
        {
            minionSelected.RemoveListener(onMinionSelected);
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

            if (selectedMinion != null && tile != null && !selectedMinion.hasMoved)
            {
                var gameTile = map.tiles.Get(selectedMinion.tilePosition);
                var path = mapService.FindPath(gameTile, tile, selectedMinion.moveDist);
                view.onTileMovePath(path);
            }
            else
            {
                view.onTileMovePath(null);
            }
        }

        private void onMinionSelected(MinionModel selectedMinion)
        {
            if (selectedMinion != null && !selectedMinion.isMoving)
            {
                var gameTile = map.tiles.Get(selectedMinion.tilePosition);
                this.selectedMinion = selectedMinion;

                view.onTileSelected(gameTile);

                if (!selectedMinion.hasMoved)
                {
                    //find movement
                    var moveTiles = mapService.GetTilesInRadius(gameTile.position, 5);
                    //take out the central one
                    moveTiles.Remove(gameTile.position);
                    view.onMovableTiles(moveTiles);
                }
            }
            else
            {
                this.selectedMinion = null;
                view.onTileSelected(null);
                view.onMovableTiles(null);
            }
        }
    }
}


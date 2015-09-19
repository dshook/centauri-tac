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

        private Tile selectedMinionPosition = null;

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

            if (selectedMinionPosition != null && tile != null)
            {
                var path = map.FindPath(selectedMinionPosition, tile);
                view.onTileMovePath(path);
            }
            else
            {
                view.onTileMovePath(null);
            }
        }

        private void onMinionSelected(MinionModel selectedMinion)
        {
            if (selectedMinion != null)
            {
                var gameTile = map.tiles.Get(selectedMinion.tilePosition);
                selectedMinionPosition = gameTile;

                view.onTileSelected(gameTile);

                //find movement
                var moveTiles = map.GetTilesInRadius(gameTile.position, 5);
                //take out the central one
                moveTiles.Remove(gameTile.position);
                view.onMovableTiles(moveTiles);
            }
            else
            {
                selectedMinionPosition = null;
                view.onTileSelected(null);
                view.onMovableTiles(null);
            }
        }
    }
}


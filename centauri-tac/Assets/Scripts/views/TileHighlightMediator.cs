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
        public IMapModel map { get; set; }

        public override void OnRegister()
        {
            view.tileHover.AddListener(onTileHover);
            minionSelected.AddListener(onMinionSelected);
            view.init();
        }

        public override void onRemove()
        {
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
        }

        private void onMinionSelected(IMinionModel selectedMinion)
        {
            if (selectedMinion != null)
            {
                var gameTile = map.tiles.Get(selectedMinion.gameObject.transform.position.ToTileCoordinates());

                view.onTileSelected(gameTile);

                //find movement
                var moveTiles = map.GetTilesInRadius(gameTile.position, 1);
                //take out the central one
                moveTiles.Remove(gameTile.position);
                view.onMovableTiles(moveTiles);
            }
            else
            {
                view.onTileSelected(null);
                view.onMovableTiles(null);
            }
        }
    }
}


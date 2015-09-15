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

        private Vector2 selectedPosition = new Vector2();

        public override void OnRegister()
        {
            view.tileHover.AddListener(onTileHover);
            view.minionSelected.AddListener(onMinionSelected);
            view.init();
        }

        public override void onRemove()
        {
        }

        void onTileHover(GameObject newHoverTile)
        {
            tileHover.Dispatch(newHoverTile);
            view.onTileHover(newHoverTile);
        }

        private void onMinionSelected(GameObject selectedMinion)
        {
            if (selectedMinion != null)
            {
                //lookup the gametile they're on
                selectedPosition.Set(selectedMinion.transform.position.x, selectedMinion.transform.position.z);
                var gameTile = map.tiles[selectedPosition];

                view.onTileSelected(gameTile.gameObject);
            }
            else
            {
                view.onTileSelected(null);
            }

            minionSelected.Dispatch(selectedMinion);
        }
    }
}


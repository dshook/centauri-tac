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

        public override void OnRegister()
        {
            view.tileHover.AddListener(onTileHover);
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
    }
}


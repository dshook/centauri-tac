using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;

namespace ctac
{
    public class PieceMovePreviewMediator : Mediator
    {
        [Inject]
        public PieceMovePreviewView view { get; set; }

        [Inject]
        public MovePathFoundSignal movePathFoundSignal { get; set; }

        public override void OnRegister()
        {
            view.init();

            movePathFoundSignal.AddListener(onMovePath);
        }

        public override void onRemove()
        {
            movePathFoundSignal.RemoveListener(onMovePath);
        }

        private void onMovePath(List<Tile> tiles)
        {
            view.onMovePath(tiles);
        }
    }
}


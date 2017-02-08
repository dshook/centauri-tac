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

        private void onMovePath(MovePathFoundModel mpfm)
        {
            if (mpfm == null)
            {
                view.onMovePath(null);
                return;
            }

            if (mpfm.startTile != null && mpfm.tiles != null)
            {
                mpfm.tiles.Insert(0, mpfm.startTile);
            }

            //should be ranged attack case
            if (mpfm.endTile != null)
            {
                view.onMovePath(new List<Tile>() { mpfm.startTile, mpfm.endTile }, true);
            }
            else
            {
                view.onMovePath(mpfm.tiles);
            }

            if (mpfm.isAttack)
            {
                view.setColor(view.attackColor);
            }
            else
            {
                view.setColor(view.defaultColor);
            }
        }
    }
}


using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;

namespace ctac
{
    public class PieceMovePreviewMediator : Mediator
    {
        [Inject]
        public PieceMovePreviewView view { get; set; }

        public override void OnRegister()
        {
            view.init();
        }

        [ListensTo(typeof(MovePathFoundSignal))]
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

            view.onMovePath(mpfm.tiles, mpfm.piece.isRanged || (mpfm.piece.statuses & Statuses.Flying) != 0);

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


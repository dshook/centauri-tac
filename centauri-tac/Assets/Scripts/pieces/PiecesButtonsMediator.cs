using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PiecesButtonsMediator : Mediator
    {
        [Inject] public PiecesButtonsView view { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }

        [Inject] public PieceHealthChangedSignal phcs { get; set; }

        PieceModel selectedPiece = null;

        public override void OnRegister()
        {
            pieceSelected.AddListener(onPieceSelected);
            view.clickSignal.AddListener(onDamagedClicked);
            view.init();
        }

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);
            view.clickSignal.RemoveListener(onDamagedClicked);
        }

        private void onDamagedClicked()
        {
            if (selectedPiece == null) return;

            var amt = -3;
            phcs.Dispatch(new PieceHealthChangeModel()
            {
                pieceId = selectedPiece.id,
                change = amt,
                newCurrentHealth = selectedPiece.health + amt,
                newCurrentArmor = 0,
                armorChange = 0
            });
        }

        private void onPieceSelected(PieceModel piece)
        {
            //don't unselect the pieces for this purpose so we'll always have one to fire off buttons on
            if (piece == null) return; 

            selectedPiece = piece;
        }

    }
}


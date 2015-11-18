using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class ClickMediator : Mediator
    {
        [Inject]
        public ClickView view { get; set; }

        [Inject]
        public PieceSelectedSignal pieceSelected { get; set; }

        [Inject]
        public AttackPieceSignal attackPiece { get; set; }

        [Inject]
        public MovePieceSignal movePiece { get; set; }

        [Inject]
        public MapModel map { get; set; }

        public override void OnRegister()
        {
            pieceSelected.AddListener(onPieceSelected);
            view.clickSignal.AddListener(onClick);
            view.init();
        }

        private PieceModel selectedPiece = null;

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);
        }

        private void onClick(GameObject clickedObject)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Piece"))
                {
                    var pieceView = clickedObject.GetComponent<PieceView>();
                    if (pieceView.piece.currentPlayerHasControl)
                    {
                        pieceSelected.Dispatch(pieceView.piece);
                    }
                    else
                    {
                        if (selectedPiece != null && !selectedPiece.hasAttacked)
                        {
                            attackPiece.Dispatch(new AttackPieceModel()
                            {
                                attackingPieceId = selectedPiece.id,
                                targetPieceId = pieceView.piece.id
                            });
                            pieceSelected.Dispatch(null);
                        }
                    }
                    return;
                }

                if (clickedObject.CompareTag("Tile"))
                {
                    var gameTile = map.tiles.Get(clickedObject.transform.position.ToTileCoordinates());

                    if (FlagsHelper.IsSet(gameTile.highlightStatus, TileHighlightStatus.Movable) && selectedPiece != null)
                    {
                        movePiece.Dispatch(selectedPiece, gameTile);
                        pieceSelected.Dispatch(null);
                    }
                }
            }
            else
            {
                pieceSelected.Dispatch(null);
            }

        }

        private void onPieceSelected(PieceModel pieceSelected)
        {
            selectedPiece = pieceSelected;
        }
    }
}


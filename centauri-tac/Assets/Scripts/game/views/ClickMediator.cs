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
        public StartSelectTargetSignal startSelectTarget { get; set; }

        [Inject]
        public CancelSelectTargetSignal cancelSelectTarget { get; set; }

        [Inject]
        public SelectTargetSignal selectTarget { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void OnRegister()
        {
            pieceSelected.AddListener(onPieceSelected);
            startSelectTarget.AddListener(onStartTarget);
            view.clickSignal.AddListener(onClick);
            view.init();
        }

        private PieceModel selectedPiece = null;

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);
            startSelectTarget.RemoveListener(onStartTarget);
        }

        private void onClick(GameObject clickedObject)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Piece"))
                {
                    var pieceView = clickedObject.GetComponent<PieceView>();
                    if (cardTarget != null)
                    {
                        debug.Log("Selected target");
                        selectTarget.Dispatch(cardTarget, pieceView.piece);
                        cardTarget = null;
                    }
                    else if (pieceView.piece.currentPlayerHasControl)
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
                if (cardTarget != null)
                {
                    debug.Log("Cancelling targeting");
                    cancelSelectTarget.Dispatch(cardTarget);
                    cardTarget = null;
                }
            }

        }

        CardModel cardTarget { get; set; }
        private void onStartTarget(CardModel card, ActionTarget at)
        {
            cardTarget = card;
        }

        private void onPieceSelected(PieceModel pieceSelected)
        {
            selectedPiece = pieceSelected;
        }
    }
}


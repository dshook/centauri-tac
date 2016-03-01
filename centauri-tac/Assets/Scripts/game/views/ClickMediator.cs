using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class ClickMediator : Mediator
    {
        [Inject] public ClickView view { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }

        [Inject] public AttackPieceSignal attackPiece { get; set; }
        [Inject] public MovePieceSignal movePiece { get; set; }
        [Inject] public RotatePieceSignal rotatePiece { get; set; }

        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public GameTurnModel turns { get; set; }

        [Inject] public IDebugService debug { get; set; }

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

                //selected should never be null but check anyways
                if (selectedPiece != null && selectedPiece.playerId == turns.currentPlayerId) {
                    if (clickedObject.CompareTag("RotateSouth"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.South));
                    }
                    if (clickedObject.CompareTag("RotateWest"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.West));
                    }
                    if (clickedObject.CompareTag("RotateNorth"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.North));
                    }
                    if (clickedObject.CompareTag("RotateEast"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.East));
                    }
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
                    cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                    cardTarget = null;
                }
            }

        }

        StartTargetModel cardTarget { get; set; }
        private void onStartTarget(StartTargetModel model)
        {
            cardTarget = model;
        }

        private void onPieceSelected(PieceModel pieceSelected)
        {
            selectedPiece = pieceSelected;
        }
    }
}


using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class ClickMediator : Mediator
    {
        [Inject] public ClickView view { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }
        [Inject] public PieceDraggingSignal pieceDragging { get; set; }

        [Inject] public AttackPieceSignal attackPiece { get; set; }
        [Inject] public MovePieceSignal movePiece { get; set; }
        [Inject] public RotatePieceSignal rotatePiece { get; set; }

        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }

        [Inject] public StartSelectAbilityTargetSignal startSelectAbilityTarget { get; set; }
        [Inject] public CancelSelectAbilityTargetSignal cancelSelectAbilityTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal selectAbilityTarget { get; set; }

        [Inject] public PieceClickedSignal pieceClicked { get; set; }
        [Inject] public TileClickedSignal tileClicked { get; set; }

        [Inject] public MessageSignal message { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public IMapService mapService { get; set; }

        public override void OnRegister()
        {
            pieceSelected.AddListener(onPieceSelected);

            startSelectTarget.AddListener(onStartTarget);
            cancelSelectTarget.AddListener(onCancelTarget);
            selectTarget.AddListener(onSelectTarget);

            startSelectAbilityTarget.AddListener(onStartAbilityTarget);
            cancelSelectAbilityTarget.AddListener(onCancelAbilityTarget);
            selectAbilityTarget.AddListener(onSelectAbilityTarget);

            view.clickSignal.AddListener(onClick);
            view.init(raycastModel);
        }

        PieceModel selectedPiece = null;
        TargetModel cardTarget = null;
        StartAbilityTargetModel abilityTarget = null;

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);

            startSelectTarget.RemoveListener(onStartTarget);
            cancelSelectTarget.RemoveListener(onCancelTarget);
            selectTarget.RemoveListener(onSelectTarget);

            startSelectAbilityTarget.RemoveListener(onStartAbilityTarget);
            cancelSelectAbilityTarget.RemoveListener(onCancelAbilityTarget);
            selectAbilityTarget.RemoveListener(onSelectAbilityTarget);
        }

        private void onClick(ClickModel clickModel)
        {
            if (clickModel.isUp)
            {
                pieceDragging.Dispatch(null);
            }

            if (clickModel.clickedObject != null)
            {
                //first check to see if the tile we clicked on has a piece, 
                //if it does, treat that the same as if we clicked the piece directly
                PieceView pieceView = null;

                if (clickModel.tile != null)
                {
                    var pieceAtTile = pieces.PieceAt(clickModel.tile.position);
                    if (pieceAtTile != null)
                    {
                        pieceView = pieceAtTile.pieceView;
                    }
                }
                if (clickModel.piece)
                {
                    pieceView = clickModel.piece;
                }

                //TODO: come back and clean up some of this heavily nested shieet
                if (pieceView != null)
                {
                    if (pieceView.piece.tags.Contains(Constants.targetPieceTag))
                    {
                        //clicking on phantom piece shouldn't do anything
                        return;
                    }
                    pieceClicked.Dispatch(pieceView);
                    if (!clickModel.isUp)
                    {
                        pieceDragging.Dispatch(pieceView.piece);
                    }

                    if (cardTarget != null || abilityTarget != null)
                    {
                    }
                    else if (pieceView.piece.currentPlayerHasControl && !clickModel.isUp)
                    {
                        pieceSelected.Dispatch(pieceView.piece);
                    }
                    else
                    {
                        //check to see if we have a valid attack, and throw a message error for all the ways its wrong
                        if ( selectedPiece != null && selectedPiece.id != pieceView.piece.id )
                        {
                            if (!FlagsHelper.IsSet(pieceView.piece.statuses, Statuses.Cloak)) {
                                if (selectedPiece.canAttack)
                                {
                                    if (mapService.isHeightPassable(
                                          mapService.Tile(selectedPiece.tilePosition),
                                          mapService.Tile(pieceView.piece.tilePosition)
                                    )) {
                                        attackPiece.Dispatch(new AttackPieceModel()
                                        {
                                            attackingPieceId = selectedPiece.id,
                                            targetPieceId = pieceView.piece.id
                                        });
                                        pieceSelected.Dispatch(null);
                                    } else {
                                        message.Dispatch(new MessageModel() { message = "Can't attack up that slope!" });
                                    }

                                }
                                else
                                {
                                    //now figure out why they can't attack
                                    if (selectedPiece.attack <= 0)
                                    {
                                        message.Dispatch(new MessageModel() { message = "Minion has no attack" });
                                    }
                                    else if (FlagsHelper.IsSet(selectedPiece.statuses, Statuses.CantAttack))
                                    {
                                        message.Dispatch(new MessageModel() { message = "Minion Can't Attack" });
                                    }
                                    else if (FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Paralyze))
                                    {
                                        message.Dispatch(new MessageModel() { message = "Paralyzed!" });
                                    }
                                    else
                                    {
                                        message.Dispatch(new MessageModel() { message = "Minions need time to prepare!" });
                                    }
                                }

                            } else {
                                message.Dispatch(new MessageModel() { message = "Can't attack the cloaked unit until they attack!" });
                            }
                            
                        }
                    }
                }

                ////selected should never be null but check anyways
                //if (selectedPiece != null && selectedPiece.playerId == players.Me.id) {
                //    if (clickModel.clickedObject.CompareTag("RotateSouth"))
                //    {
                //        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.South));
                //    }
                //    if (clickModel.clickedObject.CompareTag("RotateWest"))
                //    {
                //        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.West));
                //    }
                //    if (clickModel.clickedObject.CompareTag("RotateNorth"))
                //    {
                //        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.North));
                //    }
                //    if (clickModel.clickedObject.CompareTag("RotateEast"))
                //    {
                //        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.East));
                //    }
                //}

                if (clickModel.tile != null)
                {
                    var gameTile = clickModel.tile;

                    if (pieceView == null)
                    {
                        //only dispatch tile clicked if there wasn't a piece clicked so they're not duplicated and ambiguous
                        tileClicked.Dispatch(gameTile);
                    }

                    if (
                        cardTarget == null 
                        && FlagsHelper.IsSet(gameTile.highlightStatus, TileHighlightStatus.Movable)
                        && selectedPiece != null
                        && selectedPiece.canMove
                        )
                    {
                        movePiece.Dispatch(selectedPiece, gameTile);
                        pieceSelected.Dispatch(null);
                    }
                }
            }
            else
            {
                pieceClicked.Dispatch(null);
                tileClicked.Dispatch(null);
                pieceSelected.Dispatch(null);
                pieceDragging.Dispatch(null);
            }
        }

        private void onStartTarget(TargetModel model)
        {
            cardTarget = model;
        }
        private void onCancelTarget(CardModel card)
        {
            cardTarget = null;
        }
        private void onSelectTarget(TargetModel target)
        {
            cardTarget = null;
        }

        private void onStartAbilityTarget(StartAbilityTargetModel model)
        {
            abilityTarget = model;
        }
        private void onCancelAbilityTarget(PieceModel model)
        {
            abilityTarget = null;
        }
        private void onSelectAbilityTarget(StartAbilityTargetModel model, PieceModel piece)
        {
            abilityTarget = null;
        }

        private void onPieceSelected(PieceModel pieceSelected)
        {
            if (selectedPiece != null)
            {
                selectedPiece.isSelected = false;
            }
            selectedPiece = pieceSelected;

            if (selectedPiece != null)
            {
                selectedPiece.isSelected = true;
            }
        }

    }
}


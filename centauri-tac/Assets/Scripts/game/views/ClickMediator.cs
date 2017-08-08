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

        [Inject] public StartSelectAbilityTargetSignal startSelectAbilityTarget { get; set; }
        [Inject] public CancelSelectAbilityTargetSignal cancelSelectAbilityTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal selectAbilityTarget { get; set; }

        [Inject] public PieceClickedSignal pieceClicked { get; set; }
        [Inject] public TileClickedSignal tileClicked { get; set; }

        [Inject] public MovePathFoundSignal movePathFound { get; set; }

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

            movePathFound.AddListener(onMovePathFound);

            view.clickSignal.AddListener(onClick);
            view.init(raycastModel);
        }

        PieceModel selectedPiece = null;
        MovePathFoundModel movePath = null;
        TargetModel cardTarget = null;
        StartAbilityTargetModel abilityTarget = null;
        const float singleClickThreshold = 0.5f;

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);

            startSelectTarget.RemoveListener(onStartTarget);
            cancelSelectTarget.RemoveListener(onCancelTarget);
            selectTarget.RemoveListener(onSelectTarget);

            movePathFound.RemoveListener(onMovePathFound);

            startSelectAbilityTarget.RemoveListener(onStartAbilityTarget);
            cancelSelectAbilityTarget.RemoveListener(onCancelAbilityTarget);
            selectAbilityTarget.RemoveListener(onSelectAbilityTarget);
        }

        //For clicking anything other than a card
        private void onClick(ClickModel clickModel)
        {
            if (clickModel.clickedObject == null)
            {
                pieceClicked.Dispatch(null);
                tileClicked.Dispatch(null);
                pieceSelected.Dispatch(null);
                return;
            }

            //first check to see if the tile we clicked on has a piece, 
            //if it does, treat that the same as if we clicked the piece directly
            PieceView clickedPiece = null;

            if (clickModel.tile != null)
            {
                var pieceAtTile = pieces.PieceAt(clickModel.tile.position);
                if (pieceAtTile != null)
                {
                    clickedPiece = pieceAtTile.pieceView;
                }
            }
            if (clickModel.piece)
            {
                clickedPiece = clickModel.piece;
            }

            if (clickedPiece != null && clickModel.clickTime.HasValue && clickModel.clickTime < singleClickThreshold)
            {
                if (clickedPiece.piece.tags.Contains(Constants.targetPieceTag))
                {
                    //clicking on phantom piece shouldn't do anything
                    return;
                }

                //target click handling is elsewhere
                if (cardTarget != null || abilityTarget != null)
                {
                    pieceClicked.Dispatch(clickedPiece);
                }
                else if (clickedPiece.piece.currentPlayerHasControl)
                {
                    pieceSelected.Dispatch(clickedPiece.piece);
                    return; //return here so further actions based on this click (like move) can't be fired off from the same click
                }
                else
                {
                    //check to see if we have a valid attack, and throw a message error for all the ways its wrong
                    if ( selectedPiece != null && selectedPiece.id != clickedPiece.piece.id )
                    {
                        string errorMessage = null;
                        if (FlagsHelper.IsSet(clickedPiece.piece.statuses, Statuses.Cloak)) {
                            errorMessage = "Can't attack the cloaked unit until they attack!";
                        }
                        else if (selectedPiece.canAttack && movePath != null)
                        {
                            //find the tile the piece will end up on when attacking for melee which is the second to last tile in the list,
                            //ranged can attack up or down slopes
                            var tileToAttackFrom = movePath.tiles != null && movePath.tiles.Count >= 2 
                                ? movePath.tiles[movePath.tiles.Count - 2] 
                                : movePath.startTile;
                            if (selectedPiece.isRanged || 
                                mapService.isHeightPassable(tileToAttackFrom, mapService.Tile(clickedPiece.piece.tilePosition))
                            ){
                                attackPiece.Dispatch(new AttackPieceModel()
                                {
                                    attackingPieceId = selectedPiece.id,
                                    targetPieceId = clickedPiece.piece.id
                                });
                                pieceSelected.Dispatch(null);
                            } else {
                                errorMessage = "Can't attack up that slope!";
                            }

                        }
                        else
                        {
                            //now figure out why they can't attack
                            if (selectedPiece.attack <= 0)
                            {
                                errorMessage = "Minion has no attack";
                            }
                            else if (FlagsHelper.IsSet(selectedPiece.statuses, Statuses.CantAttack))
                            {
                                errorMessage = "Minion Can't Attack";
                            }
                            else if (FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Paralyze))
                            {
                                errorMessage = "Minion is Paralyzed!";
                            }
                            else if (selectedPiece.canAttack && movePath == null)
                            {
                                errorMessage = "Can't Get to Target";
                            }
                            else
                            {
                                errorMessage = "Minions need time to prepare!";
                            }
                        }


                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message.Dispatch(new MessageModel() { message = errorMessage });
                            return;
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

            if (clickModel.tile != null && clickModel.clickTime.HasValue && clickModel.clickTime < singleClickThreshold)
            {
                var gameTile = clickModel.tile;

                if (clickedPiece == null)
                {
                    //only dispatch tile clicked if there wasn't a piece clicked so they're not duplicated and ambiguous
                    tileClicked.Dispatch(gameTile);
                }

                if (
                    cardTarget == null
                    && FlagsHelper.IsSet(gameTile.highlightStatus, TileHighlightStatus.MoveRange)
                    && selectedPiece != null
                    && selectedPiece.canMove
                    )
                {
                    movePiece.Dispatch(selectedPiece, gameTile);
                    pieceSelected.Dispatch(null);
                }
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

        private void onMovePathFound(MovePathFoundModel mp)
        {
            movePath = mp;
        }

    }
}


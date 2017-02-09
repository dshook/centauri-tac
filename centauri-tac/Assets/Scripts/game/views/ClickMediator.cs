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
        [Inject] public UpdateTargetSignal updateTargetSignal { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }

        [Inject] public StartSelectAbilityTargetSignal startSelectAbilityTarget { get; set; }
        [Inject] public CancelSelectAbilityTargetSignal cancelSelectAbilityTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal selectAbilityTarget { get; set; }

        [Inject] public MessageSignal message { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public GameTurnModel turns { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public IMapService mapService { get; set; }

        public override void OnRegister()
        {
            pieceSelected.AddListener(onPieceSelected);
            startSelectTarget.AddListener(onStartTarget);
            cancelSelectTarget.AddListener(onCancelTarget);
            startSelectAbilityTarget.AddListener(onStartAbilityTarget);
            view.clickSignal.AddListener(onClick);
            view.init(raycastModel);
        }

        private PieceModel selectedPiece = null;

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);
            startSelectTarget.RemoveListener(onStartTarget);
            cancelSelectTarget.RemoveListener(onCancelTarget);
            startSelectAbilityTarget.RemoveListener(onStartAbilityTarget);
        }

        private void onClick(ClickModel clickModel)
        {
            if (clickModel.clickedObject != null)
            {
                if (clickModel.piece)
                {
                    var pieceView = clickModel.piece;
                    pieceDragging.Dispatch(clickModel.isUp ? null : pieceView.piece);
                    if (cardTarget != null)
                    {
                        debug.Log("Selected target");
                        cardTarget.selectedPiece = pieceView.piece;

                        var continueTarget = updateTarget(map.tiles.Get(pieceView.piece.tilePosition));

                        if (continueTarget && cardTarget.targetFulfilled)
                        {
                            selectTarget.Dispatch(cardTarget);
                            cardTarget = null;
                            pieceSelected.Dispatch(null);
                        }
                    }
                    else if (abilityTarget != null)
                    {
                        debug.Log("Selected ability target");
                        selectAbilityTarget.Dispatch(abilityTarget, pieceView.piece);
                        abilityTarget = null;
                        pieceSelected.Dispatch(null);
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
                    return;
                }

                //selected should never be null but check anyways
                if (selectedPiece != null && selectedPiece.playerId == turns.currentPlayerId) {
                    if (clickModel.clickedObject.CompareTag("RotateSouth"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.South));
                    }
                    if (clickModel.clickedObject.CompareTag("RotateWest"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.West));
                    }
                    if (clickModel.clickedObject.CompareTag("RotateNorth"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.North));
                    }
                    if (clickModel.clickedObject.CompareTag("RotateEast"))
                    {
                        rotatePiece.Dispatch(new RotatePieceModel(selectedPiece.id, Direction.East));
                    }
                }

                if (clickModel.tile != null)
                {
                    var gameTile = clickModel.tile;

                    if (cardTarget != null && cardTarget.area != null)
                    {
                        if (cardTarget.area.areaTiles != null && cardTarget.area.areaTiles.Count > 0)
                        {
                            //verify tile selected is in area
                            if (!cardTarget.area.areaTiles.Contains(gameTile.position.ToPositionModel()))
                            {
                                debug.Log("Cancelling target for outside area");
                                cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                                return;
                            }
                        }

                        var continueTargeting = updateTarget(gameTile);

                        if (continueTargeting && cardTarget.targetFulfilled)
                        {
                            selectTarget.Dispatch(cardTarget);
                            cardTarget = null;
                        }
                    } else if (
                        FlagsHelper.IsSet(gameTile.highlightStatus, TileHighlightStatus.Movable) 
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
                pieceSelected.Dispatch(null);
                pieceDragging.Dispatch(null);
                if (cardTarget != null)
                {
                    debug.Log("Cancelling targeting");
                    cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                    cardTarget = null;
                }
                else if (abilityTarget != null)
                {
                    debug.Log("Cancelling targeting");
                    cancelSelectAbilityTarget.Dispatch(abilityTarget.targetingPiece);
                    abilityTarget = null;
                }
            }

            if (clickModel.isUp)
            {
                pieceDragging.Dispatch(null);
            }
        }

        TargetModel cardTarget { get; set; }
        private void onStartTarget(TargetModel model)
        {
            cardTarget = model;
        }

        private void onCancelTarget(CardModel card)
        {
            cardTarget = null;
        }

        StartAbilityTargetModel abilityTarget { get; set; }
        private void onStartAbilityTarget(StartAbilityTargetModel model)
        {
            abilityTarget = model;
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

        private bool updateTarget(Tile tile)
        {
            if(cardTarget == null) return false;

            if(cardTarget.area == null) return true;

            if (!FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile))
            {
                cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                return false;
            }

            if (!cardTarget.selectedPosition.HasValue)
            {
                cardTarget.selectedPosition = tile.position;
            }
            else if(cardTarget.selectedPosition != tile.position)
            {
                cardTarget.selectedPivotPosition = tile.position;
            }
            updateTargetSignal.Dispatch(cardTarget);

            return true;
        }
    }
}


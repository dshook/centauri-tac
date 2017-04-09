using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class TargetMediator : Mediator
    {
        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }
        [Inject] public UpdateTargetSignal updateTargetSignal { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }

        [Inject] public StartSelectAbilityTargetSignal startSelectAbilityTarget { get; set; }
        [Inject] public CancelSelectAbilityTargetSignal cancelSelectAbilityTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal selectAbilityTarget { get; set; }

        [Inject] public PieceClickedSignal pieceClicked { get; set; }
        [Inject] public TileClickedSignal tileClicked { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public IMapService mapService { get; set; }
        [Inject] public IDebugService debug { get; set; }

        TargetModel cardTarget { get; set; }

        public override void OnRegister()
        {
            cancelSelectTarget.AddListener(onCancelSelectTarget);
            updateTargetSignal.AddListener(onUpdateTarget);
            selectTarget.AddListener(onSelectTarget);
            startSelectTarget.AddListener(onStartTarget);

            startSelectAbilityTarget.AddListener(onStartAbilityTarget);

            pieceClicked.AddListener(onPieceClicked);
            tileClicked.AddListener(onTileClicked);
        }

        public override void onRemove()
        {
            cancelSelectTarget.RemoveListener(onCancelSelectTarget);
            updateTargetSignal.AddListener(onUpdateTarget);
            selectTarget.RemoveListener(onSelectTarget);
            startSelectTarget.RemoveListener(onStartTarget);

            startSelectAbilityTarget.RemoveListener(onStartAbilityTarget);

            pieceClicked.RemoveListener(onPieceClicked);
            tileClicked.RemoveListener(onTileClicked);
        }


        private void onStartTarget(TargetModel model)
        {
            cardTarget = model;
        }

        private void onUpdateTarget(TargetModel model)
        {
        }

        private void onCancelSelectTarget(CardModel card)
        {
            cardTarget = null;
        }

        private void onSelectTarget(TargetModel targetModel)
        {

        }

        private void onPieceClicked(PieceView piece)
        {
            if(piece == null) return;

            if (cardTarget != null)
            {
                debug.Log("Selected target");
                cardTarget.selectedPiece = piece.piece;

                var continueTarget = updateTarget(piece, map.tiles.Get(piece.piece.tilePosition));

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
                selectAbilityTarget.Dispatch(abilityTarget, piece.piece);
                abilityTarget = null;
                pieceSelected.Dispatch(null);
            }
        }

        private void onTileClicked(Tile tile)
        {
            if (tile == null)
            {
                if (cardTarget != null)
                {
                    debug.Log("Cancelling targeting");
                    cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                    cardTarget = null;
                }
                
                if (abilityTarget != null)
                {
                    debug.Log("Cancelling ability targeting");
                    cancelSelectAbilityTarget.Dispatch(abilityTarget.targetingPiece);
                    abilityTarget = null;
                }
            }

            if (cardTarget != null && cardTarget.area != null)
            {
                if (cardTarget.area.areaTiles != null && cardTarget.area.areaTiles.Count > 0)
                {
                    //verify tile selected is in area
                    if (!cardTarget.area.areaTiles.Contains(tile.position.ToPositionModel()))
                    {
                        debug.Log("Cancelling target for outside area");
                        cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                        return;
                    }
                }

                var continueTargeting = updateTarget(null, tile);

                if (continueTargeting && cardTarget.targetFulfilled)
                {
                    selectTarget.Dispatch(cardTarget);
                    cardTarget = null;
                }
            }
            else if (
              cardTarget != null
              && cardTarget.targetingCard.needsTargeting(possibleActions)
            )
            {
                debug.Log("Cancelling targeting from bad selection");
                cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                cardTarget = null;
            }

        }

        StartAbilityTargetModel abilityTarget { get; set; }
        private void onStartAbilityTarget(StartAbilityTargetModel model)
        {
            abilityTarget = model;
        }

        //Returns whether or not to continue targeting
        private bool updateTarget(PieceView piece, Tile tile)
        {
            if (cardTarget == null) return false;

            if (cardTarget.area == null) return true;

            if (cardTarget.targets != null && cardTarget.targets.targetPieceIds.Count > 0 && piece != null)
            {
                if (cardTarget.targets.targetPieceIds.Contains(piece.piece.id))
                {
                    cardTarget.selectedPiece = piece.piece;
                    cardTarget.selectedPosition = piece.piece.tilePosition;
                }
            }else if (!FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile))
            {
                cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                return false;
            }

            if (!cardTarget.selectedPosition.HasValue)
            {
                cardTarget.selectedPosition = tile.position;
            }
            else if (cardTarget.selectedPosition != tile.position)
            {
                cardTarget.selectedPivotPosition = tile.position;
            }
            updateTargetSignal.Dispatch(cardTarget);

            return true;
        }
    }
}


using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class TargetMediator : Mediator
    {
        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public UpdateTargetSignal updateTargetSignal { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }

        [Inject] public CancelSelectAbilityTargetSignal cancelSelectAbilityTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal selectAbilityTarget { get; set; }

        [Inject] public ActivateCardSignal activateCard { get; set; }
        [Inject] public CancelChooseSignal cancelChoose { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }
        [Inject] public MessageSignal message { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public IMapService mapService { get; set; }
        [Inject] public IDebugService debug { get; set; }

        TargetModel cardTarget { get; set; }

        ChooseModel chooseModel;


        [ListensTo(typeof(CardSelectedSignal))]
        public void onCardSelected(CardSelectedModel cardSelected)
        {
            if (cardSelected == null)
            {
                //only cancel if we're not targeting with a choose
                if (cardTarget == null)
                {
                    if (chooseModel != null)
                    {
                        cancelChoose.Dispatch(chooseModel);
                    }
                    chooseModel = null;
                }
                return;
            }

            var card = cardSelected.card;

            //clicking on a card while targeting cancels the target;
            if (cardTarget != null && cardTarget.targetingCard != card)
            {
                cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                return;
            }


            if (card.isSpell)
            {
                if (card.needsTargeting(possibleActions))
                {
                    debug.Log("Starting Spell Targeting From Hand");
                    startSelectTarget.Dispatch(new TargetModel() {
                        targetingCard = card,
                        cardDeployPosition = null,
                        targets = possibleActions.GetActionsForCard(card.playerId, card.id),
                        cardArea = possibleActions.GetAreasForCard(card.playerId, card.id)
                    });
                    return;
                }
            }
        }

        [ListensTo(typeof(NeedsTargetSignal))]
        public void onNeedsTarget(CardModel card, Tile activatedTile)
        {
            var targets = possibleActions.GetActionsForCard(players.Me.id, card.id);
            var primaryArea = possibleActions.GetAreasForCard(players.Me.id, card.id);

            //Setup for area targeting positions
            var selectedPosition = (primaryArea != null && primaryArea.centerPosition != null) ? primaryArea.centerPosition.Vector2 : (Vector2?)null;
            if (primaryArea != null && primaryArea.selfCentered)
            {
                selectedPosition = activatedTile.position;
            }
            var pivotPosition = (primaryArea != null && primaryArea.pivotPosition != null) ? primaryArea.pivotPosition.Vector2 : (Vector2?)null;

            debug.Log("Starting targeting from activation");
            startSelectTarget.Dispatch(new TargetModel() {
                targetingCard = card,
                cardDeployPosition = activatedTile,
                targets = targets,
                cardArea = primaryArea,
                selectedPosition = selectedPosition,
                selectedPivotPosition = pivotPosition
            });
        }

        //Must need targets if the choose was updated
        [ListensTo(typeof(UpdateChooseSignal))]
        public void onUpdateChoose(ChooseModel cModel)
        {
            var selected = cModel.choices.choices
                .FirstOrDefault(x => x.cardTemplateId == cModel.chosenTemplateId.Value);

            chooseModel = cModel;
            debug.Log("Starting targeting for choose");
            startSelectTarget.Dispatch(new TargetModel() {
                targetingCard = cModel.choosingCard,
                cardDeployPosition = cModel.cardDeployPosition,
                targets = selected.targets,
                cardArea = null // not supported yet
            });
        }

        [ListensTo(typeof(StartSelectTargetSignal))]
        public void onStartTarget(TargetModel model)
        {
            cardTarget = model;
            message.Dispatch(new MessageModel() { message = "Select Your Target" });
        }

        [ListensTo(typeof(CancelSelectTargetSignal))]
        public void onCancelSelectTarget(CardModel card)
        {
            cardTarget = null;
            message.Dispatch(new MessageModel() { message = "", duration = 0f });
        }

        [ListensTo(typeof(PieceClickedSignal))]
        public void onPieceClicked(PieceView piece)
        {
            if(piece == null) return;

            if (chooseModel != null)
            {
                debug.Log("Selected target piece for choose");
                chooseModel.selectedPiece = piece.piece;
                cardTarget.selectedPiece = piece.piece;
                if (chooseModel.chooseFulfilled)
                {
                    selectTarget.Dispatch(cardTarget);
                }
            }
            else if (cardTarget != null)
            {
                debug.Log("Selected target piece");
                cardTarget.selectedPiece = piece.piece;

                var continueTarget = updateTarget(piece, map.tiles.Get(piece.piece.tilePosition));

                if (continueTarget && cardTarget.targetFulfilled)
                {
                    selectTarget.Dispatch(cardTarget);
                    pieceSelected.Dispatch(null);
                }
            }
            else if (abilityTarget != null)
            {
                debug.Log("Selected ability target piece");
                selectAbilityTarget.Dispatch(abilityTarget, piece.piece);
                abilityTarget = null;
                pieceSelected.Dispatch(null);
            }
        }

        [ListensTo(typeof(TileClickedSignal))]
        public void onTileClicked(Tile tile)
        {
            if (tile == null)
            {
                if (cardTarget != null)
                {
                    debug.Log("Cancelling targeting for clicking off tile");
                    cancelSelectTarget.Dispatch(cardTarget.targetingCard);
                }

                if (abilityTarget != null)
                {
                    debug.Log("Cancelling ability targeting");
                    cancelSelectAbilityTarget.Dispatch(abilityTarget.targetingPiece);
                    abilityTarget = null;
                }
            }

            if (cardTarget != null && cardTarget.cardArea != null)
            {
                var primaryArea = cardTarget.cardArea;
                if (primaryArea.areaTiles != null && primaryArea.areaTiles.Count > 0)
                {
                    //verify tile selected is in area
                    if (!primaryArea.areaTiles.Contains(tile.position.ToPositionModel()))
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
                }
            }

        }

        [ListensTo(typeof(PieceSpawningSignal))]
        public void onPieceSpawning(CardSelectedModel cardModel)
        {
            if (cardModel == null && cardTarget != null)
            {
                cancelSelectTarget.Dispatch(cardTarget.targetingCard);
            }
        }

        [ListensTo(typeof(SelectTargetSignal))]
        public void onSelectTarget(TargetModel targetModel)
        {
            activateCard.Dispatch(new ActivateModel() {
                cardActivated = targetModel.targetingCard,
                position = targetModel.cardDeployPosition != null ?
                    targetModel.cardDeployPosition.position :
                    targetModel.selectedPosition,
                pivotPosition = targetModel.selectedPivotPosition,
                optionalTarget = targetModel.selectedPiece,
                chooseCardTemplateId = chooseModel == null ? null : chooseModel.chosenTemplateId
            });
            cardTarget = null;
            chooseModel = null;
            message.Dispatch(new MessageModel() { message = "", duration = 0f });
        }

        //Returns whether or not to continue targeting
        public bool updateTarget(PieceView piece, Tile tile)
        {
            if (cardTarget == null) return false;

            if (cardTarget.cardArea == null) return true;

            if (cardTarget.targets != null && cardTarget.targets.targetPieceIds.Count > 0 && piece != null)
            {
                if (cardTarget.targets.targetPieceIds.Contains(piece.piece.id))
                {
                    cardTarget.selectedPiece = piece.piece;
                    cardTarget.selectedPosition = piece.piece.tilePosition;
                }
            }
            else if ((tile.highlightStatus & (TileHighlightStatus.TargetTile | TileHighlightStatus.FriendlyTargetTile | TileHighlightStatus.EnemyTargetTile)) == 0)
            {
                //cancel the target if we clicked on a non target
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

        [ListensTo(typeof(StartChooseSignal))]
        public void onStartChoose(ChooseModel c)
        {
            chooseModel = c;
        }

        [ListensTo(typeof(CancelChooseSignal))]
        public void onCancelChoose(ChooseModel c)
        {
            chooseModel = null;
        }

        [ListensTo(typeof(CardChosenSignal))]
        public void onCardChosen(ChooseModel c)
        {
            chooseModel = null;
        }

        StartAbilityTargetModel abilityTarget { get; set; }
        [ListensTo(typeof(StartSelectAbilityTargetSignal))]
        public void onStartAbilityTarget(StartAbilityTargetModel model)
        {
            abilityTarget = model;
            message.Dispatch(new MessageModel() { message = "Choose Your Target"});
        }

        [ListensTo(typeof(CancelSelectAbilityTargetSignal))]
        public void onCancelAbilityTarget(PieceModel model)
        {
            abilityTarget = null;
            message.Dispatch(new MessageModel() { message = "", duration = 0f });
        }

        [ListensTo(typeof(SelectAbilityTargetSignal))]
        public void onSelectedAbilityTarget(StartAbilityTargetModel model, PieceModel piece)
        {
            abilityTarget = null;
            message.Dispatch(new MessageModel() { message = "", duration = 0f });
        }

    }
}


using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class TargetMediator : Mediator
    {
        [Inject] public NeedsTargetSignal needsTarget { get; set; }
        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public UpdateTargetSignal updateTargetSignal { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }

        [Inject] public StartSelectAbilityTargetSignal startSelectAbilityTarget { get; set; }
        [Inject] public CancelSelectAbilityTargetSignal cancelSelectAbilityTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal selectAbilityTarget { get; set; }

        [Inject] public ActivateCardSignal activateCard { get; set; }
        [Inject] public CardSelectedSignal cardSelected { get; set; }
        [Inject] public PieceClickedSignal pieceClicked { get; set; }
        [Inject] public TileClickedSignal tileClicked { get; set; }

        [Inject] public StartChooseSignal startChoose { get; set; }
        [Inject] public UpdateChooseSignal updateChoose { get; set; }
        [Inject] public CancelChooseSignal cancelChoose { get; set; }
        [Inject] public CardChosenSignal cardChosen { get; set; }


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

        public override void OnRegister()
        {
            needsTarget.AddListener(onNeedsTarget);
            startSelectTarget.AddListener(onStartTarget);
            cancelSelectTarget.AddListener(onCancelSelectTarget);
            selectTarget.AddListener(onSelectTarget);

            startSelectAbilityTarget.AddListener(onStartAbilityTarget);
            cancelSelectAbilityTarget.AddListener(onCancelAbilityTarget);
            selectAbilityTarget.AddListener(onSelectedAbilityTarget);

            startChoose.AddListener(onStartChoose);
            updateChoose.AddListener(onUpdateChoose);
            cancelChoose.AddListener(onCancelChoose);
            cardChosen.AddListener(onCardChosen);

            cardSelected.AddListener(onCardSelected);
            pieceClicked.AddListener(onPieceClicked);
            tileClicked.AddListener(onTileClicked);
        }

        public override void onRemove()
        {
            needsTarget.RemoveListener(onNeedsTarget);
            startSelectTarget.RemoveListener(onStartTarget);
            cancelSelectTarget.RemoveListener(onCancelSelectTarget);
            selectTarget.RemoveListener(onSelectTarget);

            startSelectAbilityTarget.RemoveListener(onStartAbilityTarget);
            cancelSelectAbilityTarget.RemoveListener(onCancelAbilityTarget);
            selectAbilityTarget.RemoveListener(onSelectedAbilityTarget);

            startChoose.RemoveListener(onStartChoose);
            updateChoose.RemoveListener(onUpdateChoose);
            cancelChoose.RemoveListener(onCancelChoose);
            cardChosen.RemoveListener(onCardChosen);

            cardSelected.RemoveListener(onCardSelected);
            pieceClicked.RemoveListener(onPieceClicked);
            tileClicked.RemoveListener(onTileClicked);
        }

        private void onCardSelected(CardSelectedModel cardSelected)
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
                        area = possibleActions.GetAreasForCard(card.playerId, card.id) 
                    });
                    return;
                }
            }
        }

        private void onNeedsTarget(CardModel card, Tile activatedTile)
        {
            var targets = possibleActions.GetActionsForCard(players.Me.id, card.id);
            var area = possibleActions.GetAreasForCard(players.Me.id, card.id);

            //Setup for area targeting positions
            var selectedPosition = (area != null && area.centerPosition != null) ? area.centerPosition.Vector2 : (Vector2?)null;
            if (area != null && area.selfCentered)
            {
                selectedPosition = activatedTile.position;
            }
            var pivotPosition = (area != null && area.pivotPosition != null) ? area.pivotPosition.Vector2 : (Vector2?)null;

            debug.Log("Starting targeting from activation");
            startSelectTarget.Dispatch(new TargetModel() {
                targetingCard = card,
                cardDeployPosition = activatedTile,
                targets = targets,
                area = area,
                selectedPosition = selectedPosition,
                selectedPivotPosition = pivotPosition
            });
        }

        //Must need targets if the choose was updated
        private void onUpdateChoose(ChooseModel cModel)
        {
            var selected = cModel.choices.choices
                .FirstOrDefault(x => x.cardTemplateId == cModel.chosenTemplateId.Value);

            chooseModel = cModel;
            debug.Log("Starting targeting for choose");
            startSelectTarget.Dispatch(new TargetModel() {
                targetingCard = cModel.choosingCard,
                cardDeployPosition = cModel.cardDeployPosition,
                targets = selected.targets,
                area = null // not supported yet
            });
        }


        private void onStartTarget(TargetModel model)
        {
            cardTarget = model;
            message.Dispatch(new MessageModel() { message = "Select Your Target" });
        }

        private void onCancelSelectTarget(CardModel card)
        {
            cardTarget = null;
            message.Dispatch(new MessageModel() { message = "", duration = 0f });
        }

        private void onPieceClicked(PieceView piece)
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

        private void onTileClicked(Tile tile)
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
                }
            }
            else if (
              cardTarget != null
              && cardTarget.targetingCard.needsTargeting(possibleActions)
            )
            {
                debug.Log("Cancelling targeting from bad selection");
                cancelSelectTarget.Dispatch(cardTarget.targetingCard);
            }

        }

        private void onSelectTarget(TargetModel targetModel)
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
            }
            else if (!FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile))
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

        private void onStartChoose(ChooseModel c)
        {
            chooseModel = c;
        }

        private void onCancelChoose(ChooseModel c)
        {
            chooseModel = null;
        }

        private void onCardChosen(ChooseModel c)
        {
            chooseModel = null;
        }

        StartAbilityTargetModel abilityTarget { get; set; }
        private void onStartAbilityTarget(StartAbilityTargetModel model)
        {
            abilityTarget = model;
            message.Dispatch(new MessageModel() { message = "Choose Your Target"});
        }

        private void onCancelAbilityTarget(PieceModel model)
        {
            abilityTarget = null;
            message.Dispatch(new MessageModel() { message = "", duration = 0f });
        }

        private void onSelectedAbilityTarget(StartAbilityTargetModel model, PieceModel piece)
        {
            abilityTarget = null;
            message.Dispatch(new MessageModel() { message = "", duration = 0f });
        }

    }
}


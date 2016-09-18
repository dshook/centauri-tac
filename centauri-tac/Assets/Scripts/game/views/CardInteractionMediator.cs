using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class CardInteractionMediator : Mediator
    {
        [Inject] public CardInteractionView view { get; set; }

        [Inject] public CardSelectedSignal cardSelected { get; set; }
        [Inject] public CardHoveredSignal cardHovered { get; set; }
        [Inject] public ActivateCardSignal activateCard { get; set; }
        [Inject] public ActionMessageSignal message { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }
        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }

        [Inject] public StartChooseSignal startChoose { get; set; }
        [Inject] public CancelChooseSignal cancelChoose { get; set; }
        [Inject] public CardChosenSignal cardChosen { get; set; }

        [Inject] public CardsModel cards { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public GameTurnModel turns { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }

        private CardModel draggedCard = null;

        [Inject]
        public IDebugService debug { get; set; }

        //for card targeting
        private TargetModel startTargetModel;

        //and for card choosing
        private ChooseModel chooseModel;

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.activateSignal.AddListener(onActivate);
            view.hoverSignal.AddListener(onHover);
            selectTarget.AddListener(onSelectedTarget);
            cancelSelectTarget.AddListener(onTargetCancel);
            activateCard.AddListener(onCardActivated);
            view.init(raycastModel);
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.activateSignal.RemoveListener(onActivate);
            view.hoverSignal.RemoveListener(onHover);
            selectTarget.RemoveListener(onSelectedTarget);
            activateCard.RemoveListener(onCardActivated);
            cancelSelectTarget.RemoveListener(onTargetCancel);
        }


        private void onClick(GameObject clickedObject, Vector3 point)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Card"))
                {
                    var cardView = clickedObject.GetComponent<CardView>();

                    //Choose card interactions
                    if (chooseModel != null && cardView.card.tags.Contains(Constants.chooseCardTag))
                    {
                        chooseModel.chosenTemplateId = cardView.card.cardTemplateId;
                        cardChosen.Dispatch(chooseModel);
                        if (chooseModel.chooseFulfilled)
                        {
                            activateCard.Dispatch(new ActivateModel()
                            {
                                cardActivated = chooseModel.choosingCard,
                                optionalTarget = null,
                                position = chooseModel.cardDeployPosition.position,
                                pivotPosition = null,
                                chooseCardTemplateId = chooseModel.chosenTemplateId
                            });
                            chooseModel = null;
                        }
                        else
                        {
                            var selected = chooseModel.choices.choices
                                .FirstOrDefault(x => x.cardTemplateId == chooseModel.chosenTemplateId.Value);
                            //if the choice isn't fulfilled yet it must mean it needs targets
                            startTargetModel = new TargetModel()
                            {
                                targetingCard = chooseModel.choosingCard,
                                cardDeployPosition = chooseModel.cardDeployPosition,
                                targets = selected.targets,
                                area = null // not supported yet
                            };
                            Invoke("StartSelectTargets", 0.10f);
                            return;
                        }
                        return;
                    }

                    if (cardView.card.isSpell)
                    {
                        var targets = possibleActions.GetActionsForCard(turns.currentPlayerId, cardView.card.id);
                        var area = possibleActions.GetAreasForCard(turns.currentPlayerId, cardView.card.id);
                        if (targets != null || area != null)
                        {
                            startTargetModel = new TargetModel()
                            {
                                targetingCard = cardView.card,
                                cardDeployPosition = null,
                                targets = targets,
                                area = area
                            };
                            Invoke("StartSelectTargets", 0.10f);
                            return;
                        }
                    }

                    draggedCard = cardView.card;
                    pieceSelected.Dispatch(null); 
                    cardSelected.Dispatch(new CardSelectedModel() { card = draggedCard, point = point });
                }
            }
            else
            {
                draggedCard = null;
                cardSelected.Dispatch(null);

                //only cancel if we're not targeting with a choose
                if (startTargetModel == null)
                {
                    if (chooseModel != null)
                    {
                        cancelChoose.Dispatch(chooseModel);
                    }
                    chooseModel = null;
                }
            }
        }

        private void onActivate(GameObject activated)
        {
            if (activated == null || draggedCard == null)
            {
                cardSelected.Dispatch(null);
                return;
            }

            if (activated.CompareTag("Tile"))
            {
                //check for appropriate resources
                if (draggedCard.cost > playerResources.resources[draggedCard.playerId])
                {
                    message.Dispatch(new MessageModel() { message = "Not enough energy to play!" }, new SocketKey(turns.currentTurnClientId, "game"));
                    return;
                }

                var gameTile = map.tiles.Get(activated.transform.position.ToTileCoordinates());

                if (draggedCard.isChoose(possibleActions))
                {
                    chooseModel = new ChooseModel()
                    {
                        choosingCard = draggedCard,
                        cardDeployPosition = gameTile,
                        choices = possibleActions.GetChoiceCards(draggedCard.playerId, draggedCard.id)
                    };
                    debug.Log("Starting choose");
                    startChoose.Dispatch(chooseModel);
                }
                else if (draggedCard.needsTargeting(possibleActions))
                {
                    var targets = possibleActions.GetActionsForCard(turns.currentPlayerId, draggedCard.id);
                    var area = possibleActions.GetAreasForCard(turns.currentPlayerId, draggedCard.id);

                    var selectedPosition = (area != null && area.centerPosition != null) ? area.centerPosition.Vector2 : (Vector2?)null;
                    if (area != null && area.selfCentered)
                    {
                        selectedPosition = gameTile.position;
                    }
                    var pivotPosition = (area != null && area.pivotPosition != null) ? area.pivotPosition.Vector2 : (Vector2?)null;

                    //record state we need to maintain for subsequent clicks then dispatch the start target
                    startTargetModel = new TargetModel()
                    {
                        targetingCard = draggedCard,
                        cardDeployPosition = gameTile,
                        targets = targets,
                        area = area,
                        selectedPosition = selectedPosition,
                        selectedPivotPosition = pivotPosition
                    };

                    //delay sending off the start select target signal till the card deselected event has cleared
                    Invoke("StartSelectTargets", 0.10f);
                }
                else
                {
                    activateCard.Dispatch(new ActivateModel()
                    {
                        cardActivated = draggedCard,
                        position = gameTile.position,
                        optionalTarget = null
                    });
                }
            }
        }

        private void StartSelectTargets()
        {
            debug.Log("Starting targeting");
            startSelectTarget.Dispatch(startTargetModel);
        }

        private void onTargetCancel(CardModel card)
        {
            startTargetModel = null;
            if (chooseModel != null)
            {
                debug.Log("Cancelling choose from target cancel");
                cancelChoose.Dispatch(chooseModel);
            }
            chooseModel = null;
            cardSelected.Dispatch(null);
        }

        private void onSelectedTarget(TargetModel targetModel)
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
            startTargetModel = null;
            chooseModel = null;
        }

        private void onCardActivated(ActivateModel a)
        {
            cardSelected.Dispatch(null);
        }

        private CardView lastHoveredCard = null;
        private void onHover(GameObject hoveredObject)
        {
            if (hoveredObject != null)
            {
                if (hoveredObject.CompareTag("Card"))
                {
                    var cardView = hoveredObject.GetComponent<CardView>();
                    if (cardView != null && cardView != lastHoveredCard )
                    {
                        //break out and don't hover if it hasn't been added to the hand of cards yet
                        if (!cards.Cards.Contains(cardView.card))
                        {
                            return;
                        }
                        lastHoveredCard = cardView;
                        cardHovered.Dispatch(cardView.card);

                    }
                }
            }
            else
            {
                if (lastHoveredCard != null)
                {
                    lastHoveredCard = null;
                    cardHovered.Dispatch(null);
                }
            }
        }
    }
}


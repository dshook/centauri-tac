using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

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

        [Inject] public CardsModel cards { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public GameTurnModel turns { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public PlayerResourcesModel playerResources { get; set; }

        private CardModel draggedCard = null;

        [Inject]
        public IDebugService debug { get; set; }

        //for card targeting
        private StartTargetModel startTargetModel;

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.activateSignal.AddListener(onActivate);
            view.hoverSignal.AddListener(onHover);
            selectTarget.AddListener(onSelectedTarget);
            cancelSelectTarget.AddListener(onTargetCancel);
            view.init();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.activateSignal.RemoveListener(onActivate);
            view.hoverSignal.RemoveListener(onHover);
            selectTarget.RemoveListener(onSelectedTarget);
            cancelSelectTarget.RemoveListener(onTargetCancel);
        }


        private void onClick(GameObject clickedObject)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Card"))
                {
                    var cardView = clickedObject.GetComponent<CardView>();

                    if (cardView.card.tags.Contains("Spell"))
                    {
                        var targets = possibleActions.GetActionsForCard(turns.currentPlayerId, cardView.card.id);
                        if (targets != null)
                        {
                            startTargetModel = new StartTargetModel()
                            {
                                targetingCard = cardView.card,
                                cardDeployPosition = null,
                                targets = targets
                            };
                            Invoke("StartSelectTargets", 0.10f);
                            return;
                        }
                    }

                    draggedCard = cardView.card;
                    pieceSelected.Dispatch(null); 
                    cardSelected.Dispatch(draggedCard);
                }
            }
            else
            {
                draggedCard = null;
                cardSelected.Dispatch(null);
            }
        }

        private void onActivate(GameObject activated)
        {
            if (activated != null && draggedCard != null)
            {
                if (activated.CompareTag("Tile"))
                {
                    //check for appropriate resources
                    if (draggedCard.cost > playerResources.resources[draggedCard.playerId])
                    {
                        message.Dispatch(new MessageModel() { message = "Not enough energy to play!" }, new SocketKey(turns.currentTurnClientId, "game"));
                        return;
                    }

                    var gameTile = map.tiles.Get(activated.transform.position.ToTileCoordinates());

                    var targets = possibleActions.GetActionsForCard(turns.currentPlayerId, draggedCard.id);
                    if (targets != null && targets.targetPieceIds.Count >= 1)
                    {
                        //record state we need to maintain for subsequent clicks then dispatch the start target
                        startTargetModel = new StartTargetModel()
                        {
                            targetingCard = draggedCard,
                            cardDeployPosition = gameTile,
                            targets = targets
                        };

                        //delay sending off the start select target signal till the card deselected event has cleared
                        Invoke("StartSelectTargets", 0.10f);
                    }
                    else
                    {
                        activateCard.Dispatch(new ActivateModel() {
                            cardActivated = draggedCard,
                            tilePlayedAt = gameTile,
                            optionalTarget = null
                        });
                    }
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
        }

        private void onSelectedTarget(StartTargetModel targetModel, PieceModel piece)
        {
            activateCard.Dispatch(new ActivateModel() {
                cardActivated = targetModel.targetingCard,
                tilePlayedAt = targetModel.cardDeployPosition,
                optionalTarget = piece
            });
            startTargetModel = null;
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


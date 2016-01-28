using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class CardInteractionMediator : Mediator
    {
        [Inject]
        public CardInteractionView view { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public CardHoveredSignal cardHovered { get; set; }

        [Inject]
        public ActivateCardSignal activateCard { get; set; }

        [Inject]
        public StartSelectTargetSignal startSelectTarget { get; set; }

        [Inject]
        public SelectTargetSignal selectTarget { get; set; }

        [Inject]
        public CancelSelectTargetSignal cancelSelectTarget { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public PossibleActionsModel possibleActions { get; set; }

        [Inject]
        public GameTurnModel turns { get; set; }

        [Inject]
        public MapModel map { get; set; }

        private CardModel draggedCard = null;

        [Inject]
        public IDebugService debug { get; set; }

        //for card targeting
        private Tile cardDeployPosition;
        private int timesDeselected = 0;
        private ActionTarget targets;
        private CardModel targetCard;

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.activateSignal.AddListener(onActivate);
            view.hoverSignal.AddListener(onHover);
            view.init();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.activateSignal.RemoveListener(onActivate);
            view.hoverSignal.RemoveListener(onHover);
        }


        private void onClick(GameObject clickedObject)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Card"))
                {
                    var cardView = clickedObject.GetComponent<CardView>();

                    draggedCard = cardView.card;
                    cardSelected.Dispatch(draggedCard);
                }
            }
            else
            {
                //swallow the first deselect event that happens for the targeting
                //so the card can stop being dragged
                if (cardDeployPosition != null && timesDeselected == 0)
                {
                    debug.Log("Cancelling targeting");
                    cardDeployPosition = null;
                    targetCard = null;
                    cancelSelectTarget.Dispatch(draggedCard);
                    view.EndTarget();
                }
                timesDeselected++;
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
                    var gameTile = map.tiles.Get(activated.transform.position.ToTileCoordinates());

                    targets = possibleActions.GetForCard(turns.currentPlayerId, draggedCard.id);
                    if (targets != null)
                    {
                        //record state we need to maintain for subsequent clicks then dispatch the start target
                        cardDeployPosition = gameTile;
                        targetCard = draggedCard;
                        view.StartTarget();

                        //delay sending off the start select target signal till the card deselected event has cleared
                        Invoke("DelaySelectTargets", 0.10f);
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
                else if (activated.CompareTag("Piece") && cardDeployPosition != null)
                {
                    debug.Log("Selected target");
                    //we should have just selected the piece for the target now
                    var pieceView = activated.GetComponent<PieceView>();
                    selectTarget.Dispatch(targetCard, pieceView.piece);
                    activateCard.Dispatch(new ActivateModel() {
                        cardActivated = targetCard,
                        tilePlayedAt = cardDeployPosition,
                        optionalTarget = pieceView.piece
                    });
                    cardDeployPosition = null;
                    view.EndTarget();
                }
            }
        }

        private void DelaySelectTargets()
        {
            debug.Log("Starting targeting");
            timesDeselected = 0;
            startSelectTarget.Dispatch(targetCard, targets);
            view.StartTarget();
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


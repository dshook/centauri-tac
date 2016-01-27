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
                else if (clickedObject.CompareTag("Piece") && cardDeployPosition != null)
                {
                    debug.Log("Selected target");
                    //we should have just selected the piece for the target now
                    var pieceView = clickedObject.GetComponent<PieceView>();
                    selectTarget.Dispatch(draggedCard, pieceView.piece);
                    activateCard.Dispatch(new ActivateModel() {
                        cardActivated = draggedCard,
                        tilePlayedAt = cardDeployPosition,
                        optionalTarget = pieceView.piece
                    });
                    cardDeployPosition = null;
                }
            }
            else
            {
                if (cardDeployPosition != null)
                {
                    debug.Log("Cancelling targeting");
                    cardDeployPosition = null;
                    cancelSelectTarget.Dispatch(draggedCard);
                }
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

                    var targets = possibleActions.GetForCard(turns.currentPlayerId, draggedCard.id);
                    if (targets != null)
                    {
                        debug.Log("Starting targeting");
                        //record state we need to maintain for subsequent clicks then dispatch the start target
                        cardDeployPosition = gameTile;
                        view.StartTarget();
                        startSelectTarget.Dispatch(draggedCard, targets);
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


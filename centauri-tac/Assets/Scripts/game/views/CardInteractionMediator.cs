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
        [Inject] public MessageSignal message { get; set; }

        [Inject] public PieceSpawningSignal pieceSpawning { get; set; }
        [Inject] public PieceSelectedSignal pieceSelected { get; set; }

        [Inject] public NeedsTargetSignal needsTarget { get; set; }

        [Inject] public StartChooseSignal startChoose { get; set; }
        [Inject] public UpdateChooseSignal updateChoose { get; set; }
        [Inject] public CancelChooseSignal cancelChoose { get; set; }
        [Inject] public CardChosenSignal cardChosen { get; set; }

        [Inject] public TileClickedSignal tileClicked { get; set; }

        [Inject] public CardsModel cards { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        private CardModel draggedCard = null;

        [Inject] public IDebugService debug { get; set; }
        [Inject] public ISoundService sounds { get; set; }
        [Inject] public IMapService mapService { get; set; }

        //for card targeting
        private TargetModel cardTarget;

        //and for card choosing
        private ChooseModel chooseModel;

        public override void OnRegister()
        {
            view.cardClickSignal.AddListener(onCardClick);
            view.activateSignal.AddListener(onActivate);
            view.hoverSignal.AddListener(onHover);

            view.init(raycastModel);
        }

        public override void OnRemove()
        {
            view.cardClickSignal.RemoveListener(onCardClick);
            view.activateSignal.RemoveListener(onActivate);
            view.hoverSignal.RemoveListener(onHover);
        }

        //for clicking on a card directly
        private void onCardClick(GameObject clickedObject, Vector3 point)
        {
            if(clickedObject == null)
            {
                draggedCard = null;
                cardSelected.Dispatch(null);
                pieceSpawning.Dispatch(null);

                return;
            }

            var cardView = clickedObject.GetComponent<CardView>();
            if (cardView == null) { return; }
            
            draggedCard = cardView.card;

            //Choose card interactions
            if (chooseModel != null && draggedCard.tags.Contains(Constants.chooseCardTag))
            {
                chooseModel.chosenTemplateId = draggedCard.cardTemplateId;
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
                    updateChoose.Dispatch(chooseModel);
                    return;
                }
                return;
            }

            pieceSelected.Dispatch(null);
            if (draggedCard.isMinion)
            {
                pieceSpawning.Dispatch(new CardSelectedModel() { card = draggedCard, point = point });
            }
            cardSelected.Dispatch(new CardSelectedModel() { card = draggedCard, point = point });
        }

        //when you try to activate a card either by the click and drag click up or a click on a tile/piece
        private void onActivate(GameObject activated)
        {
            sounds.PlaySound("playCard");
            var itWorked = doActivateWork(activated);
            if (itWorked)
            {
                view.ClearDrag();
                cardSelected.Dispatch(null);
            }
        }

        //returns whether or not the activate was a good one or not
        private bool doActivateWork(GameObject activated)
        {
            if (activated == null || draggedCard == null)
            {
                cardSelected.Dispatch(null);
                return false;
            }

            if (!activated.CompareTag("Tile"))
            {
                return false;
            }

            //if targeting is in progress the target mediator will handle the activate
            if (cardTarget != null)
            {
                return true;
            }

            //check for appropriate resources
            if (draggedCard.cost > playerResources.resources[draggedCard.playerId])
            {
                message.Dispatch(new MessageModel() { message = "Not enough energy to play!", duration = 1f });
                return false;
            }

            var gameTile = map.tiles.Get(activated.transform.position.ToTileCoordinates());

            if (draggedCard.isMinion)
            {
                //basic sanity checks first
                if (gameTile.unpassable)
                {
                    message.Dispatch(new MessageModel() { message = "That location doesn't look safe!", duration = 1f });
                    return false;
                }
                if (pieces.PieceAt(gameTile.position) != null)
                {
                    message.Dispatch(new MessageModel() { message = "That location is already occupied!", duration = 1f });
                    return false;
                }
                var kingDist = mapService.KingDistance(pieces.Hero(draggedCard.playerId).tilePosition, gameTile.position);
                if (kingDist > 1)
                {
                    message.Dispatch(new MessageModel() { message = "You must play your minions close to your hero!", duration = 1f });
                    return false;
                }
            }

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
                return true;
            }

            if (draggedCard.needsTargeting(possibleActions) && cardTarget == null)
            {
                needsTarget.Dispatch(draggedCard, gameTile);
                return true;
            }

            activateCard.Dispatch(new ActivateModel()
            {
                cardActivated = draggedCard,
                position = gameTile.position,
                optionalTarget = null
            });
            return true;
        }

        [ListensTo(typeof(StartSelectTargetSignal))]
        public void onStartTarget(TargetModel target)
        {
            cardTarget = target;
        }

        [ListensTo(typeof(CancelSelectTargetSignal))]
        public void onTargetCancel(CardModel card)
        {
            cardTarget = null;
            if (chooseModel != null)
            {
                debug.Log("Cancelling choose from target cancel");
                cancelChoose.Dispatch(chooseModel);
            }
            chooseModel = null;
            view.ClearDrag();
            cardSelected.Dispatch(null);
        }

        [ListensTo(typeof(SelectTargetSignal))]
        public void onSelectedTarget(TargetModel t)
        {
            cardTarget = null;
        }

        [ListensTo(typeof(ActivateCardSignal))]
        public void onCardActivated(ActivateModel a)
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
                    if (cardView != null && cardView != lastHoveredCard && players.Me.id == cardView.card.playerId )
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


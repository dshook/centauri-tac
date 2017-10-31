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
        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }

        [Inject] public StartChooseSignal startChoose { get; set; }
        [Inject] public UpdateChooseSignal updateChoose { get; set; }
        [Inject] public CancelChooseSignal cancelChoose { get; set; }
        [Inject] public CardChosenSignal cardChosen { get; set; }

        [Inject] public TileClickedSignal tileClicked { get; set; }

        [Inject] public GameFinishedSignal gameFinished { get; set; }

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

        //for card targeting
        private TargetModel targetModel;

        //and for card choosing
        private ChooseModel chooseModel;

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.activateSignal.AddListener(onActivate);
            view.hoverSignal.AddListener(onHover);

            startSelectTarget.AddListener(onStartTarget);
            selectTarget.AddListener(onSelectedTarget);
            cancelSelectTarget.AddListener(onTargetCancel);
            gameFinished.AddListener(onGameFinished);

            activateCard.AddListener(onCardActivated);
            view.init(raycastModel);
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.activateSignal.RemoveListener(onActivate);
            view.hoverSignal.RemoveListener(onHover);

            startSelectTarget.RemoveListener(onStartTarget);
            selectTarget.RemoveListener(onSelectedTarget);
            cancelSelectTarget.RemoveListener(onTargetCancel);
            gameFinished.RemoveListener(onGameFinished);

            activateCard.RemoveListener(onCardActivated);
        }

        //for clicking on a card directly
        private void onClick(GameObject clickedObject, Vector3 point)
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
            if (targetModel != null)
            {
                return true;
            }

            //check for appropriate resources
            if (draggedCard.cost > playerResources.resources[draggedCard.playerId])
            {
                message.Dispatch(new MessageModel() { message = "Not enough energy to play!" });
                return false;
            }

            var gameTile = map.tiles.Get(activated.transform.position.ToTileCoordinates());

            if (draggedCard.isMinion)
            {
                //basic sanity checks first
                if (gameTile.unpassable)
                {
                    message.Dispatch(new MessageModel() { message = "That location doesn't look safe!" });
                    return false;
                }
                if (pieces.PieceAt(gameTile.position) != null)
                {
                    message.Dispatch(new MessageModel() { message = "That location is already occupied!" });
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

            if (draggedCard.needsTargeting(possibleActions) && targetModel == null)
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

        private void onStartTarget(TargetModel target)
        {
            targetModel = target;
        }

        private void onTargetCancel(CardModel card)
        {
            targetModel = null;
            if (chooseModel != null)
            {
                debug.Log("Cancelling choose from target cancel");
                cancelChoose.Dispatch(chooseModel);
            }
            chooseModel = null;
            view.ClearDrag();
            cardSelected.Dispatch(null);
        }

        private void onSelectedTarget(TargetModel t)
        {
            targetModel = null;
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

        private void onGameFinished(GameFinishedModel gf)
        {
            view.Disable();
        }
    }
}


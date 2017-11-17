using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class ClickMediator : Mediator
    {
        [Inject] public ClickView view { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }
        [Inject] public PieceClickedSignal pieceClicked { get; set; }
        [Inject] public TileClickedSignal tileClicked { get; set; }
        [Inject] public PieceSpawningSignal pieceSpawning { get; set; }

        [Inject] public AttackPieceSignal attackPiece { get; set; }
        [Inject] public MessageSignal message { get; set; }
        [Inject] public MovePieceSignal movePiece { get; set; }

        [Inject] public CardSelectedSignal cardSelected { get; set; }
        [Inject] public CardHoveredSignal cardHovered { get; set; }
        [Inject] public ActivateCardSignal activateCard { get; set; }

        [Inject] public NeedsTargetSignal needsTarget { get; set; }

        [Inject] public StartChooseSignal startChoose { get; set; }
        [Inject] public UpdateChooseSignal updateChoose { get; set; }
        [Inject] public CancelChooseSignal cancelChoose { get; set; }
        [Inject] public CardChosenSignal cardChosen { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public PlayerResourcesModel playerResources { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public IMapService mapService { get; set; }
        [Inject] public ISoundService sounds { get; set; }


        CardModel draggedCard = null;
        Vector3 draggedCardInitialPosition;
        float draggedCardTime = 0f;

        PieceModel selectedPiece = null;
        MovePathFoundModel movePath = null;
        TargetModel cardTarget = null;
        ChooseModel chooseModel;
        StartAbilityTargetModel abilityTarget = null;
        const float singleClickThreshold = 0.5f;

        enum ClickMode
        {
            Neutral,
            CardSingleClicked,
            CardDragging
        }

        ClickMode clickMode = ClickMode.Neutral;


        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.cardClickSignal.AddListener(onCardClick);
            view.hoverSignal.AddListener(onHover);
            view.init(raycastModel);
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.cardClickSignal.RemoveListener(onCardClick);
            view.hoverSignal.RemoveListener(onHover);
        }

        //For clicking anything other than a card
        private void onClick(ClickModel clickModel)
        {
            switch (clickMode) {
                case ClickMode.Neutral:
                    handleNeutralClick(clickModel);
                    break;
                case ClickMode.CardSingleClicked:
                case ClickMode.CardDragging:
                    handleClickWithCard(clickModel);
                    break;
            }
        }

        //for clicking on a card directly
        private void onCardClick(CardClickModel cardClick)
        {
            var cardView = cardClick != null ? cardClick.clickedCard.GetComponent<CardView>() : null;
            if(cardClick == null || cardView == null)
            {
                DeselectCard();
                return;
            }

            //Test activation for dragged or clicked on spells
            if (
                cardView.card.isSpell 
                && (clickMode == ClickMode.CardDragging || clickMode == ClickMode.CardSingleClicked)
                && (Time.time - draggedCardTime) > singleClickThreshold
            ){

                if (!CardInsideCircle(cardView.rectTransform.anchoredPosition3D, 1.5f))
                {
                    if (doActivateWork(null))
                    {
                        sounds.PlaySound("playCard");
                        DeselectCard();
                        return;
                    }
                }
                else
                {
                    DeselectCard();
                    return;
                }
            }

            if (cardClick.isUp)
            {
                //if we get an up click on an already activated card this should mean that we just single clicked on it to activate
                //and we don't need to do anything else
                if (cardView.card.activated)
                {
                    return;
                }
                //clicking up on the same dragged card
                if (draggedCard != null && cardView.card == draggedCard && (Time.time - draggedCardTime) < singleClickThreshold)
                {
                    clickMode = ClickMode.CardSingleClicked;
                    //shouldn't need to do anything further here since it should be handled on the initial click

                    //activate if spell card has been dragged x distance
                    //if less than that distance, deselect the spell
                    return;
                }
            }
            else
            {
                clickMode = ClickMode.CardDragging;
            }

            draggedCard = cardView.card;
            draggedCardInitialPosition = cardView.gameObject.transform.position;
            draggedCardTime = Time.time;

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
                pieceSpawning.Dispatch(new CardSelectedModel() { card = draggedCard, point = cardClick.position });
            }
            cardSelected.Dispatch(new CardSelectedModel() { card = draggedCard, point = cardClick.position });
        }

        private void handleNeutralClick(ClickModel clickModel)
        {
            if (clickModel == null || clickModel.clickedObject == null)
            {
                pieceClicked.Dispatch(null);
                tileClicked.Dispatch(null);
                pieceSelected.Dispatch(null);
                return;
            }

            //first check to see if the tile we clicked on has a piece, 
            //if it does, treat that the same as if we clicked the piece directly
            PieceView clickedPiece = null;

            if (clickModel.tile != null)
            {
                var pieceAtTile = pieces.PieceAt(clickModel.tile.position);
                if (pieceAtTile != null)
                {
                    clickedPiece = pieceAtTile.pieceView;
                }
            }
            if (clickModel.piece)
            {
                clickedPiece = clickModel.piece;
            }

            if (clickedPiece != null)
            {
                if (clickedPiece.piece.tags.Contains(Constants.targetPieceTag))
                {
                    //clicking on phantom piece shouldn't do anything
                    return;
                }

                //target click handling is elsewhere
                if (cardTarget != null || abilityTarget != null)
                {
                    pieceClicked.Dispatch(clickedPiece);
                }
                else if (clickedPiece.piece.currentPlayerHasControl)
                {
                    pieceSelected.Dispatch(clickedPiece.piece);
                    return; //return here so further actions based on this click (like move) can't be fired off from the same click
                }
                else
                {
                    //check to see if we have a valid attack, and throw a message error for all the ways its wrong
                    if ( selectedPiece != null && selectedPiece.id != clickedPiece.piece.id )
                    {
                        string errorMessage = null;
                        if (FlagsHelper.IsSet(clickedPiece.piece.statuses, Statuses.Cloak))
                        {
                            errorMessage = "Can't attack the cloaked unit until they attack!";
                        }
                        else if (clickedPiece.piece.isMoving)
                        {
                            //debug.Log("Clicked on moving piece");
                        }
                        else if (selectedPiece.canAttack && movePath != null)
                        {
                            //find the tile the piece will end up on when attacking for melee which is the second to last tile in the list,
                            //ranged can attack up or down slopes
                            var tileToAttackFrom = movePath.tiles != null && movePath.tiles.Count >= 2
                                ? movePath.tiles[movePath.tiles.Count - 2]
                                : movePath.startTile;
                            if (selectedPiece.isRanged ||
                                mapService.isHeightPassable(tileToAttackFrom, mapService.Tile(clickedPiece.piece.tilePosition))
                            )
                            {
                                attackPiece.Dispatch(new AttackPieceModel()
                                {
                                    attackingPieceId = selectedPiece.id,
                                    targetPieceId = clickedPiece.piece.id
                                });
                                pieceSelected.Dispatch(null);
                            }
                            else
                            {
                                errorMessage = "Can't attack up that slope!";
                            }

                        }
                        else
                        {
                            //now figure out why they can't attack
                            if (selectedPiece.attack <= 0)
                            {
                                errorMessage = "Minion has no attack";
                            }
                            else if (FlagsHelper.IsSet(selectedPiece.statuses, Statuses.CantAttack))
                            {
                                errorMessage = "Minion Can't Attack";
                            }
                            else if (FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Paralyze))
                            {
                                errorMessage = "Minion is Paralyzed!";
                            }
                            else if (selectedPiece.canAttack && movePath == null)
                            {
                                errorMessage = "Can't Get to Target";
                            }
                            else
                            {
                                errorMessage = "Minions need time to prepare!";
                            }
                        }


                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message.Dispatch(new MessageModel() { message = errorMessage, duration = 1f });
                            return;
                        }
                    }
                }
            }

            if (clickModel.tile != null)
            {
                var gameTile = clickModel.tile;

                if (clickedPiece == null)
                {
                    //only dispatch tile clicked if there wasn't a piece clicked so they're not duplicated and ambiguous
                    tileClicked.Dispatch(gameTile);
                }

                if (
                    cardTarget == null
                    && FlagsHelper.IsSet(gameTile.highlightStatus, TileHighlightStatus.MoveRange)
                    && selectedPiece != null
                    && selectedPiece.canMove
                    )
                {
                    movePiece.Dispatch(selectedPiece, gameTile);
                    pieceSelected.Dispatch(null);
                }
            }
        }

        private void handleClickWithCard(ClickModel clickModel)
        {
            if (clickModel == null || clickModel.clickedObject == null)
            {
                DeselectCard();
                return;
            }

            //first check to see if we clicked on a piece or tile
            //this logic is very similar to the neutral click handling but differs in some important ways
            PieceView clickedPiece = null;

            if (clickModel.tile != null)
            {
                var pieceAtTile = pieces.PieceAt(clickModel.tile.position);
                if (pieceAtTile != null)
                {
                    clickedPiece = pieceAtTile.pieceView;
                }
            }
            if (clickModel.piece)
            {
                clickedPiece = clickModel.piece;
            }

            if (clickedPiece != null)
            {
                if (clickedPiece.piece.tags.Contains(Constants.targetPieceTag))
                {
                    //clicking on phantom piece shouldn't do anything
                    return;
                }

                //target click handling is elsewhere
                if (cardTarget != null || abilityTarget != null)
                {
                    pieceClicked.Dispatch(clickedPiece);
                }
            }

            if (doActivateWork(clickModel.tile))
            {
                sounds.PlaySound("playCard");
                DeselectCard();
            }
        }

        //when you try to activate a card either by the click and drag click up or a click on a tile/piece
        //returns whether or not the activate was a good one or not
        private bool doActivateWork(Tile gameTile)
        {
            if (draggedCard == null)
            {
                cardSelected.Dispatch(null);
                return false;
            }

            //skip for already activated
            if (draggedCard.activated)
            {
                return false;
            }

            //if targeting is in progress the target mediator will handle the activate
            if (cardTarget != null)
            {
                if (gameTile != null)
                {
                    tileClicked.Dispatch(gameTile);
                }
                return true;
            }

            //check for appropriate resources
            if (draggedCard.cost > playerResources.resources[draggedCard.playerId])
            {
                message.Dispatch(new MessageModel() { message = "Not enough energy to play!", duration = 1f });
                return false;
            }

            if (draggedCard.isMinion)
            {
                //basic sanity checks first
                if (gameTile == null || gameTile.unpassable)
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
                position = gameTile != null ? gameTile.position : (Vector2?)null,
                optionalTarget = null
            });
            return true;
        }

        CardView lastHoveredCard = null;
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

        private void DeselectCard()
        {
            draggedCard = null;
            cardSelected.Dispatch(null);
            pieceSpawning.Dispatch(null);
            clickMode = ClickMode.Neutral;
        }

        private bool CardInsideCircle(Vector3 position, float boost)
        {
            var distToCenter = Vector2.Distance(position, Constants.cardCircleCenter);
            return distToCenter < Constants.cardCircleRadius * boost;
        }

        [ListensTo(typeof(StartSelectAbilityTargetSignal))]
        public void onStartAbilityTarget(StartAbilityTargetModel model)
        {
            abilityTarget = model;
        }
        [ListensTo(typeof(CancelSelectAbilityTargetSignal))]
        public void onCancelAbilityTarget(PieceModel model)
        {
            abilityTarget = null;
        }
        [ListensTo(typeof(SelectAbilityTargetSignal))]
        public void onSelectAbilityTarget(StartAbilityTargetModel model, PieceModel piece)
        {
            abilityTarget = null;
        }

        [ListensTo(typeof(PieceSelectedSignal))]
        public void onPieceSelected(PieceModel pieceSelected)
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

        [ListensTo(typeof(MovePathFoundSignal))]
        public void onMovePathFound(MovePathFoundModel mp)
        {
            movePath = mp;
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
            draggedCard = null;
            cardSelected.Dispatch(null);
        }

        [ListensTo(typeof(StartSelectTargetSignal))]
        public void onStartTarget(TargetModel model)
        {
            cardTarget = model;
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
    }
}


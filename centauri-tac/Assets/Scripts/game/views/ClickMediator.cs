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
        float draggedCardTime = 0f;
        bool clickUpCardInHand = false;

        PieceModel selectedPiece = null;
        MovePathFoundModel movePath = null;
        TargetModel cardTarget = null;
        ChooseModel chooseModel;
        StartAbilityTargetModel abilityTarget = null;
        const float singleClickThreshold = 0.1f;

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
            if (draggedCard != null)
            {
                handleClickWithCard(clickModel);
            }
            else
            {
                handleNeutralClick(clickModel);
            }
        }

        //for clicking on a card directly
        private void onCardClick(CardClickModel cardClick)
        {
            var cardView = cardClick != null ? cardClick.clickedCard.GetComponent<CardView>() : null;
            if(cardClick == null || cardView == null)
            {
                cardSelected.Dispatch(null);
                pieceSpawning.Dispatch(null);
                return;
            }

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
                    updateChoose.Dispatch(chooseModel);
                    return;
                }
                return;
            }

            //Everything else below this should rely on the card being in a players hand
            if(!cardView.card.inHand){ return; }

            //Test activation for dragged or clicked on spells
            if (
                cardView.card.isSpell
                && (draggedCard != null)
                && (Time.time - draggedCardTime) > singleClickThreshold
            ){

                if (!CardInsideCircle(cardView.rectTransform.anchoredPosition3D, 1.5f))
                {
                    if (doActivateWork(null))
                    {
                        sounds.PlaySound("playCard");
                        cardSelected.Dispatch(null);
                        return;
                    }
                }
                else
                {
                    cardSelected.Dispatch(null);
                    return;
                }
            }

            if (cardClick.isUp)
            {
                clickUpCardInHand = draggedCard != null;

                //if we get an up click on an already activated card this should mean that we just single clicked on it to activate
                //and we don't need to do anything else
                if (cardView.card.activated)
                {
                    return;
                }
                //clicking up on the same dragged card
                if (draggedCard != null && cardView.card == draggedCard && (Time.time - draggedCardTime) < singleClickThreshold)
                {
                    //shouldn't need to do anything further here since it should be handled on the initial click
                    return;
                }
                if ((Time.time - draggedCardTime) < singleClickThreshold) {
                    return;
                }
                //dragging map and ending up on a card
                if (draggedCard == null && cardClick.isDrag)
                {
                    return;
                }
            }

            pieceSelected.Dispatch(null);
            if (cardView.card.isMinion)
            {
                pieceSpawning.Dispatch(new CardSelectedModel() { card = cardView.card, point = cardClick.position });
            }
            cardSelected.Dispatch(new CardSelectedModel() { card = cardView.card, point = cardClick.position });
        }

        //Handle clicking on something NOT a card when a card has not been selected
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
                else if( selectedPiece != null && selectedPiece.id != clickedPiece.piece.id && !clickedPiece.piece.currentPlayerHasControl )
                {
                    //check to see if we have a valid attack, and throw a message error for all the ways its wrong
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
                        else if (FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Petrify))
                        {
                            errorMessage = "Minion is Paralyzed!";
                        }
                        else if (selectedPiece.canAttack && movePath == null)
                        {
                            errorMessage = "Can't Get to Target";
                        }
                        else if(selectedPiece.age == 0 && !FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Charge))
                        {
                            errorMessage = "Minions need time to prepare!";
                        }
                        else if (selectedPiece.attackCount >= selectedPiece.maxAttacks)
                        {
                            errorMessage = "Minion has already attacked this turn!";
                        }
                        else
                        {
                            //Shouldn't be able to hit this one anymore
                            errorMessage = "Minion Can't Attack";
                        }
                    }


                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        message.Dispatch(new MessageModel() { message = errorMessage, duration = 1f });
                        return;
                    }
                }
                else
                {
                    pieceSelected.Dispatch(clickedPiece.piece);
                    return; //return here so further actions based on this click (like move) can't be fired off from the same click
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
                    && !clickModel.isDrag
                    )
                {
                    movePiece.Dispatch(selectedPiece, gameTile);
                    pieceSelected.Dispatch(null);
                }
            }
        }

        //Clicking on something that's NOT a card when a card has been selected
        private void handleClickWithCard(ClickModel clickModel)
        {
            if (clickModel == null || clickModel.clickedObject == null)
            {
                cardSelected.Dispatch(null);
                return;
            }

            //If deploying a minion ignore drags around the map,
            //but only for when you clicked on the card in your hand then tried to drag around the map
            //not for when you click and drag from your hand to the spot on the board
            if(clickModel.isDrag && (draggedCard != null && draggedCard.isMinion) && clickUpCardInHand){
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
                var allowableDistance = (draggedCard.statuses & Statuses.Airdrop) != 0 ? 4 : 1;
                if (kingDist > allowableDistance)
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
                selectedPiece.pieceView.UpdateHpBar();
                selectedPiece.pieceView.FocusHpBar(false);
            }
            selectedPiece = pieceSelected;

            if (selectedPiece != null)
            {
                selectedPiece.isSelected = true;
                selectedPiece.pieceView.UpdateHpBar();
                selectedPiece.pieceView.FocusHpBar(true);
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
                chooseModel = null;
            }

            if (draggedCard != null)
            {
                draggedCard = null;
                cardSelected.Dispatch(null);
            }
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

        [ListensTo(typeof(CardSelectedSignal))]
        public void onCardSelected(CardSelectedModel cardSelected)
        {
            if (cardSelected != null)
            {
                draggedCard = cardSelected.card;
                draggedCardTime = Time.time;
                clickUpCardInHand = false;
            }
            else
            {
                draggedCard = null;
                draggedCardTime = Time.time;
                lastHoveredCard = null;
            }
        }

        [ListensTo(typeof(ActivateCardSignal))]
        public void onCardActivated(ActivateModel a)
        {
            cardSelected.Dispatch(null);
        }
    }
}


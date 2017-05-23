using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class CardsMediator : Mediator
    {
        [Inject] public CardsView view { get; set; }

        [Inject] public CardSelectedSignal cardSelected { get; set; }
        [Inject] public CardHoveredSignal cardHovered { get; set; }

        [Inject] public ActivateCardSignal activateCard { get; set; }

        [Inject] public DestroyCardSignal destroyCard { get; set; }
        [Inject] public CardDestroyedSignal cardDestroyed { get; set; }

        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public CardDrawnSignal cardDrawn { get; set; }
        [Inject] public CardDrawShownSignal cardDrawShown { get; set; }
        [Inject] public CardDiscardedSignal cardDiscarded { get; set; }
        [Inject] public CardGivenSignal cardGiven { get; set; }
        [Inject] public PlayerResourceSetSignal playerResourceSet { get; set; }
        [Inject] public CardBuffSignal cardBuffed { get; set; }
        [Inject] public CancelChooseSignal cancelChoose { get; set; }
        [Inject] public CardChosenSignal cardChosen { get; set; }
        [Inject] public ServerQueueProcessEnd qpc { get; set; }

        [Inject] public CardsModel cards { get; set; }
        [Inject] public GameTurnModel gameTurn { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public AnimationQueueModel animationQueue { get; set; }
        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public ISoundService sounds { get; set; }

        public override void OnRegister()
        {
            view.init(PlayerCards(), OpponentCards());
            cardSelected.AddListener(onCardSelected);
            cardHovered.AddListener(onCardHovered);
            activateCard.AddListener(onCardActivated);
            destroyCard.AddListener(onDestroyCard);
            cardDestroyed.AddListener(onCardDestroyed);
            cardDrawn.AddListener(onCardDrawn);
            cardGiven.AddListener(onCardGiven);
            cardDrawShown.AddListener(onCardDrawnShown);
            cardDiscarded.AddListener(onCardDiscarded);
            cardBuffed.AddListener(onBuff);
            turnEnded.AddListener(onTurnEnded);
            playerResourceSet.AddListener(onPlayerResourceSet);
            qpc.AddListener(onQueueProcessComplete);

            cancelChoose.AddListener(cleanupChooseCards);
            cardChosen.AddListener(cleanupChooseCards);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
            cardHovered.RemoveListener(onCardHovered);
            destroyCard.RemoveListener(onDestroyCard);
            activateCard.RemoveListener(onCardActivated);
            cardDestroyed.RemoveListener(onCardDestroyed);
            cardDrawn.RemoveListener(onCardDrawn);
            cardGiven.RemoveListener(onCardGiven);
            cardDrawShown.RemoveListener(onCardDrawnShown);
            cardDiscarded.RemoveListener(onCardDiscarded);
            turnEnded.RemoveListener(onTurnEnded);
            cardBuffed.AddListener(onBuff);
            playerResourceSet.RemoveListener(onPlayerResourceSet);
            qpc.RemoveListener(onQueueProcessComplete);

            cancelChoose.RemoveListener(cleanupChooseCards);
            cardChosen.RemoveListener(cleanupChooseCards);
        }

        private void onCardSelected(CardSelectedModel cardModel)
        {
            var needsArrow = true;
            if (cardModel != null && cardModel.card.isSpell)
            {
                needsArrow = cardModel.card.needsTargeting(possibleActions);
            }
            view.onCardSelected(cardModel, needsArrow);
        }

        private void onCardHovered(CardModel card)
        {
            view.onCardHovered(card);
        }

        private void onCardActivated(ActivateModel act)
        {
            act.cardActivated.activated = true;
        }

        private void onDestroyCard(int cardId)
        {
            var card = cards.Cards.FirstOrDefault(c => c.id == cardId);
            if (card == null)
            {
                debug.LogError("Could not destroy card from card Id");
                return;
            }
            animationQueue.Add(new CardsView.CardDestroyedAnim()
            {
                card = card,
                cardDestroyed = cardDestroyed
            });
        }

        private void onCardDestroyed(CardModel card)
        {
            cards.Cards.Remove(card);
            Destroy(card.gameObject);
            view.init(PlayerCards(), OpponentCards());
        }

        private void onCardDrawn(CardModel card)
        {
            //animate the card going to the right hand, based on who's turn it is and if it's hotseat vs net
            var isOpponent = card.playerId != players.Me.id;
            animationQueue.Add(new CardsView.DrawCardAnim()
            {
                card = card,
                cardDrawn = cardDrawShown,
                isOpponentCard = isOpponent,
                sounds = sounds
            });
        }

        private void onCardGiven(CardModel card)
        {
            //animate the card going to the right hand, based on who's turn it is and if it's hotseat vs net
            var isOpponent = card.playerId != players.Me.id;

            animationQueue.Add(new CardsView.GiveCardAnim()
            {
                card = card,
                isOpponentCard = isOpponent 
            });

            onCardDrawn(card);
        }

        private void onCardDrawnShown(CardModel card)
        {
            cards.Cards.Add(card);
            UpdateCardsPlayableStatus(cards.Cards);
            view.init(PlayerCards(), OpponentCards());
        }

        private void onCardDiscarded(CardModel card)
        {
            var isOpponent = card.playerId != players.Me.id;
            card.activated = true;
            animationQueue.Add(new CardsView.DiscardCardAnim()
            {
                card = card,
                destroyCard = destroyCard,
                isOpponentCard = isOpponent
            });
        }

        private void onBuff(CardBuffModel cardBuff)
        {
            var card = cards.Card(cardBuff.cardId);

            if (cardBuff.cost != null)
            {
                animationQueue.Add(
                    new CardView.UpdateTextAnim()
                    {
                        text = card.cardView.costText,
                        textGO = card.cardView.costGO,
                        current = card.cost,
                        original = card.baseCost,
                        change = cardBuff.cost.Value
                    }
                );
            }
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            view.init(PlayerCards(), OpponentCards());
            UpdateCardsPlayableStatus(cards.Cards);
        }

        private void onQueueProcessComplete(int t)
        {
            //update text on all cards when the queue completes to catch updates 
            //to playable status, cost, etc
            foreach (var card in cards.Cards)
            {
                card.cardView.UpdateText(possibleActions.GetSpellDamage(card.playerId));
            }
        }

        private List<CardModel> PlayerCards()
        {
            if(cards == null || cards.Cards == null) return new List<CardModel>();

            return cards.Cards.Where(c => c.playerId == players.Me.id).ToList();
        }
        private List<CardModel> OpponentCards()
        {
            if(cards == null || cards.Cards == null) return new List<CardModel>();

            return cards.Cards.Where(c => c.playerId != players.Me.id).ToList();
        }

        private void onPlayerResourceSet(SetPlayerResourceModel resource)
        {
            UpdateCardsPlayableStatus(cards.Cards);
        }

        private void UpdateCardsPlayableStatus(List<CardModel> cards)
        {
            foreach (var card in cards)
            {
                if ( card.playerId == players.Me.id
                   && playerResources.resources.ContainsKey(card.playerId) 
                   && card.cost <= playerResources.resources[card.playerId])
                {
                    //Check for spells that need targets but don't have any
                    var cardTargets = possibleActions.GetActionsForCard(card.playerId, card.id);
                    if (card.isSpell && cardTargets != null && cardTargets.targetPieceIds.Count == 0)
                    {
                        card.playable = false;
                        continue;
                    }

                    card.playable = true;
                }
                else
                {
                    card.playable = false;
                }
            }
        }

        private void cleanupChooseCards(ChooseModel chooseModel)
        {
            debug.Log("Cleanup on aisle choose card");

            var cardParent = contextView.transform.FindChild(Constants.cardCanvas);

            var leftCard = cardParent.FindChild("Left Choice Card");
            var rightCard = cardParent.FindChild("Right Choice Card");

            if (leftCard == null || rightCard == null)
            {
                debug.Log("Couldn't find one of the choice cards in cancel");
                return;
            }
            Destroy(leftCard.gameObject);
            Destroy(rightCard.gameObject);


        }
    }
}


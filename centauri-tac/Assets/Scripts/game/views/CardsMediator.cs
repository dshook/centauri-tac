using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class CardsMediator : Mediator
    {
        [Inject] public CardsView view { get; set; }

        [Inject] public DestroyCardSignal destroyCard { get; set; }
        [Inject] public CardDestroyedSignal cardDestroyed { get; set; }
        [Inject] public CardDrawShownSignal cardDrawShown { get; set; }

        [Inject] public CancelChooseSignal cancelChoose { get; set; }
        [Inject] public CardChosenSignal cardChosen { get; set; }

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

            cancelChoose.AddListener(cleanupChooseCards);
            cardChosen.AddListener(cleanupChooseCards);
        }

        public override void OnRemove()
        {
            cancelChoose.RemoveListener(cleanupChooseCards);
            cardChosen.RemoveListener(cleanupChooseCards);
        }

        [ListensTo(typeof(CardSelectedSignal))]
        public void onCardSelected(CardSelectedModel cardModel)
        {
            var needsArrow = true;
            if (cardModel != null && cardModel.card.isSpell)
            {
                needsArrow = cardModel.card.needsTargeting(possibleActions);
            }
            view.onCardSelected(cardModel, needsArrow);
        }

        [ListensTo(typeof(CardHoveredSignal))]
        public void onCardHovered(CardModel card)
        {
            view.onCardHovered(card);
        }

        [ListensTo(typeof(ActivateCardSignal))]
        public void onCardActivated(ActivateModel act)
        {
            act.cardActivated.activated = true;
        }

        [ListensTo(typeof(DestroyCardSignal))]
        public void onDestroyCard(int cardId)
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

        [ListensTo(typeof(CardDestroyedSignal))]
        public void onCardDestroyed(CardModel card)
        {
            cards.Cards.Remove(card);
            Destroy(card.gameObject);
            view.init(PlayerCards(), OpponentCards());
        }

        [ListensTo(typeof(CardDrawnSignal))]
        public void onCardDrawn(CardModel card)
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

        [ListensTo(typeof(CardGivenSignal))]
        public void onCardGiven(CardModel card)
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

        [ListensTo(typeof(CardDrawShownSignal))]
        public void onCardDrawnShown(CardModel card)
        {
            cards.Cards.Add(card);
            UpdateCardsPlayableStatus(cards.Cards);
            view.init(PlayerCards(), OpponentCards());
        }

        [ListensTo(typeof(CardDiscardedSignal))]
        public void onCardDiscarded(CardModel card)
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

        [ListensTo(typeof(CardBuffSignal))]
        public void onBuff(CardBuffModel cardBuff)
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

        [ListensTo(typeof(TurnEndedSignal))]
        public void onTurnEnded(GameTurnModel turns)
        {
            view.init(PlayerCards(), OpponentCards());
            UpdateCardsPlayableStatus(cards.Cards);
        }

        [ListensTo(typeof(ServerQueueProcessEnd))]
        public void onQueueProcessComplete(int t)
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

        [ListensTo(typeof(PlayerResourceSetSignal))]
        public void onPlayerResourceSet(SetPlayerResourceModel resource)
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

            var cardParent = contextView.transform.Find(Constants.cardCanvas);

            var leftCard = cardParent.Find("Left Choice Card");
            var rightCard = cardParent.Find("Right Choice Card");

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


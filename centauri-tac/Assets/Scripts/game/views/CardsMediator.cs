using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class CardsMediator : Mediator
    {
        [Inject]
        public CardsView view { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public CardHoveredSignal cardHovered { get; set; }

        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        [Inject]
        public ActivateCardSignal activateCard { get; set; }

        [Inject]
        public DestroyCardSignal destroyCard { get; set; }

        [Inject]
        public CardDestroyedSignal cardDestroyed { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        [Inject]
        public CardDrawnSignal cardDrawn { get; set; }

        [Inject]
        public CardDrawShownSignal cardDrawShown { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        [Inject]
        public PlayerResourcesModel playerResources { get; set; }

        [Inject]
        public PlayerResourceSetSignal playerResourceSet { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void OnRegister()
        {
            view.init(GetCurrentPlayerCards());
            cardSelected.AddListener(onCardSelected);
            cardHovered.AddListener(onCardHovered);
            activateCard.AddListener(onCardActivated);
            destroyCard.AddListener(onDestroyCard);
            cardDestroyed.AddListener(onCardDestroyed);
            cardDrawn.AddListener(onCardDrawn);
            cardDrawShown.AddListener(onCardDrawnShown);
            turnEnded.AddListener(onTurnEnded);
            playerResourceSet.AddListener(onPlayerResourceSet);
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
            cardDrawShown.RemoveListener(onCardDrawnShown);
            turnEnded.RemoveListener(onTurnEnded);
            playerResourceSet.RemoveListener(onPlayerResourceSet);
        }

        private void onCardSelected(CardModel card)
        {
            view.onCardSelected(card);
        }

        private void onCardHovered(CardModel card)
        {
            view.onCardHovered(card);
        }

        private CardModel lastActivatedCard = null;
        private void onCardActivated(ActivateModel act)
        {
            lastActivatedCard = act.cardActivated;
        }

        private void onDestroyCard(int cardId)
        {
            if (lastActivatedCard == null || lastActivatedCard.id != cardId)
            {
                debug.LogError("Could not destroy card from card Id");
                return;
            }
            animationQueue.Add(new CardsView.CardDestroyedAnim()
            {
                card = lastActivatedCard,
                cardDestroyed = cardDestroyed
            });
        }

        private void onCardDestroyed(CardModel card)
        {
            cards.Cards.Remove(card);
            view.init(GetCurrentPlayerCards());
        }

        private void onCardDrawn(CardModel card)
        {
            //skip animation for non active players
            if (card.playerId != gameTurn.currentPlayerId)
            {
                cards.Cards.Add(card);
                UpdateCardsPlayableStatus(cards.Cards);
                return;
            }

            animationQueue.Add(new CardsView.DrawCardAnim()
            {
                card = card,
                cardDrawn = cardDrawShown
            });
        }

        private void onCardDrawnShown(CardModel card)
        {
            cards.Cards.Add(card);
            UpdateCardsPlayableStatus(cards.Cards);
            view.init(GetCurrentPlayerCards());
        }

        private void onTurnEnded()
        {
            view.init(GetCurrentPlayerCards());
        }

        private List<CardModel> GetCurrentPlayerCards()
        {
            if(cards == null || cards.Cards == null) return new List<CardModel>();
            //hide non player cards
            var nonPlayerCards = cards.Cards.Where(c => c.playerId != gameTurn.currentPlayerId).ToList();
            foreach (var card in nonPlayerCards)
            {
                card.gameObject.SetActive(false);
            }

            //enable player cards
            var playerCards = cards.Cards.Where(c => c.playerId == gameTurn.currentPlayerId).ToList();
            foreach (var card in playerCards)
            {
                card.gameObject.SetActive(true);
            }

            return playerCards;
        }

        private void onPlayerResourceSet(SetPlayerResourceModel resource)
        {
            UpdateCardsPlayableStatus(cards.Cards);
        }

        private void UpdateCardsPlayableStatus(List<CardModel> cards)
        {
            foreach (var card in cards)
            {
                if (playerResources.resources.ContainsKey(card.playerId) 
                   && card.cost <= playerResources.resources[card.playerId])
                {
                    card.playable = true;
                }
                else
                {
                    card.playable = false;
                }
            }
        }
    }
}


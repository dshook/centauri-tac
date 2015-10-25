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
        public AnimationQueueModel animationQueue { get; set; }

        [Inject]
        public DestroyCardSignal destroyCard { get; set; }

        [Inject]
        public CardDestroyedSignal cardDestroyed { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        public override void OnRegister()
        {
            view.init(GetCurrentPlayerCards());
            cardSelected.AddListener(onCardSelected);
            destroyCard.AddListener(onDestroyCard);
            cardDestroyed.AddListener(onCardDestroyed);
            turnEnded.AddListener(onTurnEnded);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
            destroyCard.RemoveListener(onDestroyCard);
            cardDestroyed.RemoveListener(onCardDestroyed);
            turnEnded.RemoveListener(onTurnEnded);
        }

        private void onCardSelected(CardModel card)
        {
            view.onCardSelected(card);
        }

        private void onDestroyCard(CardModel card)
        {
            animationQueue.Add(new CardsView.CardDestroyedAnim()
            {
                card = card,
                cardDestroyed = cardDestroyed
            });
        }

        private void onCardDestroyed(CardModel card)
        {
            cards.Cards.Remove(card);
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
    }
}

using strange.extensions.mediation.impl;
using ctac.signals;

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

        public override void OnRegister()
        {
            view.init(cards);
            cardSelected.AddListener(onCardSelected);
            destroyCard.AddListener(onDestroyCard);
            cardDestroyed.AddListener(onCardDestroyed);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
            destroyCard.RemoveListener(onDestroyCard);
            cardDestroyed.RemoveListener(onCardDestroyed);
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
    }
}


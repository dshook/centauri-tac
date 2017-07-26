using ctac.signals;
using strange.extensions.mediation.impl;

namespace ctac
{
    public class DeckEditHolderMediator : Mediator
    {
        [Inject] public DeckEditHolderView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        [Inject] public EditDeckSignal editDeck { get; set; }

        [Inject] public AddCardToDeckSignal addCardToDeck { get; set; }
        [Inject] public RemoveCardFromDeckSignal removeCardFromDeck { get; set; }

        public override void OnRegister()
        {
            addCardToDeck.AddListener(onAddToDeck);
            removeCardFromDeck.AddListener(onRemoveFromDeck);
            editDeck.AddListener(onEditDeck);
            view.init(loader);
        }

        public override void onRemove()
        {
            addCardToDeck.RemoveListener(onAddToDeck);
            removeCardFromDeck.RemoveListener(onRemoveFromDeck);
            editDeck.RemoveListener(onEditDeck);
        }

        public void Update()
        {
        }

        private void onAddToDeck(CardModel card)
        {
            view.addCard(card);
        }

        private void onRemoveFromDeck(CardModel card)
        {
            view.removeCard(card);
        }

        private void onEditDeck(DeckModel deck)
        {
            view.EditDeck(deck);
        }

    }
}


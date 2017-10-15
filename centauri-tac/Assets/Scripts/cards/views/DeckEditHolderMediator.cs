using ctac.signals;
using strange.extensions.mediation.impl;

namespace ctac
{
    public class DeckEditHolderMediator : Mediator
    {
        [Inject] public DeckEditHolderView view { get; set; }
        [Inject] public CardDirectory directory { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        [Inject] public EditDeckSignal editDeck { get; set; }
        [Inject] public SavingDeckSignal saveDeck { get; set; }
        [Inject] public CancelDeckSignal cancelDeck { get; set; }
        [Inject] public DeckSavedSignal deckSaved { get; set; }
        [Inject] public DeleteDeckSignal deleteDeck { get; set; }

        [Inject] public AddCardToDeckSignal addCardToDeck { get; set; }
        [Inject] public RemoveCardFromDeckSignal removeCardFromDeck { get; set; }

        //is a deck currently being edited?
        bool active = false;

        public override void OnRegister()
        {
            addCardToDeck.AddListener(onAddToDeck);
            cancelDeck.AddListener(onCancelDeck);
            saveDeck.AddListener(onSaveDeck);
            removeCardFromDeck.AddListener(onRemoveFromDeck);
            editDeck.AddListener(onEditDeck);
            deckSaved.AddListener(onDeckSaved);
            deleteDeck.AddListener(onDeleteDeck);
            view.init(loader, directory);
        }

        public override void onRemove()
        {
            addCardToDeck.RemoveListener(onAddToDeck);
            cancelDeck.RemoveListener(onCancelDeck);
            saveDeck.RemoveListener(onSaveDeck);
            removeCardFromDeck.RemoveListener(onRemoveFromDeck);
            editDeck.RemoveListener(onEditDeck);
            deckSaved.RemoveListener(onDeckSaved);
            deleteDeck.RemoveListener(onDeleteDeck);
        }

        public void Update()
        {
        }

        private void onAddToDeck(CardModel card)
        {
            if (!active) return;

            view.addCard(card.cardTemplateId);
        }

        private void onRemoveFromDeck(CardModel card)
        {
            view.removeCard(card);
        }

        private void onEditDeck(DeckModel deck)
        {
            view.EditDeck(deck);
            active = true;
        }

        private void onSaveDeck(DeckModel deck)
        {
            view.SaveDeck(deck);
        }

        private void onCancelDeck()
        {
            view.EditDeck(null);
            active = false;
        }

        private void onDeckSaved(DeckModel deck, SocketKey key)
        {
            active = false;
        }

        private void onDeleteDeck(DeckModel deck)
        {
            active = false;
        }

    }
}


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

        //is a deck currently being edited?
        bool active = false;

        public override void OnRegister()
        {
            view.init(loader, directory);
        }

        [ListensTo(typeof(AddCardToDeckSignal))]
        public void onAddToDeck(CardModel card)
        {
            if (!active) return;

            view.addCard(card.cardTemplateId);
        }

        [ListensTo(typeof(RemoveCardFromDeckSignal))]
        public void onRemoveFromDeck(CardModel card)
        {
            view.removeCard(card);
        }

        [ListensTo(typeof(EditDeckSignal))]
        public void onEditDeck(DeckModel deck)
        {
            view.EditDeck(deck);
            active = true;
        }

        [ListensTo(typeof(SaveDeckSignal))]
        public void onSaveDeck(DeckModel deck)
        {
            view.SaveDeck(deck);
        }

        [ListensTo(typeof(CancelDeckSignal))]
        public void onCancelDeck()
        {
            view.EditDeck(null);
            active = false;
        }

        [ListensTo(typeof(DeckSavedSignal))]
        public void onDeckSaved(DeckModel deck, SocketKey key)
        {
            active = false;
        }

        [ListensTo(typeof(DeleteDeckSignal))]
        public void onDeleteDeck(DeckModel deck)
        {
            active = false;
        }

    }
}


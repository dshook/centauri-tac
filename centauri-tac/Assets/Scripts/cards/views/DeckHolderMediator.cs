using ctac.signals;
using strange.extensions.mediation.impl;

namespace ctac
{
    public class DeckHolderMediator : Mediator
    {
        [Inject] public DeckHolderView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        public override void OnRegister()
        {
            view.init(loader);
        }

        [ListensTo(typeof(NewDeckSignal))]
        public void onAddToDeck(DeckModel deck)
        {
            view.addDeck(deck);
        }

        [ListensTo(typeof(DeckDeletedSignal))]
        public void onRemoveFromDeck(DeckModel deck)
        {
            view.removeCard(deck);
        }

        [ListensTo(typeof(DeckSavedSignal))]
        public void onDeckSaved(DeckModel deck, SocketKey key)
        {
            view.deckSaved(deck);
        }

        [ListensTo(typeof(ClearDecksSignal))]
        public void onClearDecks()
        {
            view.clearDecks();
        }

        [ListensTo(typeof(SelectDeckSignal))]
        public void onSelectDeck(DeckModel deck)
        {
            view.selectDeck(deck);
        }

    }
}


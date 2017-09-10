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

        [Inject] public NewDeckSignal newDeckSignal { get; set; }
        [Inject] public DeckDeletedSignal removeDeckSignal { get; set; }
        [Inject] public DeckSavedSignal deckSaved { get; set; }


        public override void OnRegister()
        {
            newDeckSignal.AddListener(onAddToDeck);
            removeDeckSignal.AddListener(onRemoveFromDeck);
            deckSaved.AddListener(onDeckSaved);
            view.init(loader);
        }

        public override void onRemove()
        {
            newDeckSignal.RemoveListener(onAddToDeck);
            removeDeckSignal.RemoveListener(onRemoveFromDeck);
            deckSaved.RemoveListener(onDeckSaved);
        }

        public void Update()
        {
        }

        private void onAddToDeck(DeckModel deck)
        {
            view.addDeck(deck);
        }

        private void onRemoveFromDeck(DeckModel deck)
        {
            view.removeCard(deck);
        }

        private void onDeckSaved(DeckModel deck, SocketKey key)
        {
            view.deckSaved(deck);
        }

    }
}


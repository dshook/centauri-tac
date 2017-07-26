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
        [Inject] public RemoveDeckSignal removeDeckSignal { get; set; }

        public override void OnRegister()
        {
            newDeckSignal.AddListener(onAddToDeck);
            removeDeckSignal.AddListener(onRemoveFromDeck);
            view.init(loader);
        }

        public override void onRemove()
        {
            newDeckSignal.RemoveListener(onAddToDeck);
            removeDeckSignal.RemoveListener(onRemoveFromDeck);
        }

        public void Update()
        {
        }

        private void onAddToDeck(DeckModel deck)
        {
            view.addDeck(deck);
        }

        private void onRemoveFromDeck(DeckModel card)
        {
            view.removeCard(card);
        }

    }
}


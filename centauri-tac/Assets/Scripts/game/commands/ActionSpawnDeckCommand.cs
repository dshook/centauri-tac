using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;
using strange.extensions.context.api;

namespace ctac
{
    public class ActionSpawnDeckCommand : Command
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public SpawnDeckModel spawnDeck { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public DecksModel decks { get; set; }

        [Inject]
        public DeckSpawnedSignal deckSpawned { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }
        
        private GameObject _cardPrefab { get; set; }
        private GameObject cardPrefab
        {
            get
            {
                if (_cardPrefab == null)
                {
                    _cardPrefab = Resources.Load("Card") as GameObject;
                }
                return _cardPrefab;
            }
        }

        private Vector3 spawnPosition = new Vector3(10000,10000, 0);

        public override void Execute()
        {
            if (!processedActions.Verify(spawnDeck.id)) return;

            //holding area that all cards go into to start with until the deck mediator sorts them out
            var DeckGO = GameObject.Find("Deck");

            for (var c = 0; c < spawnDeck.cards; c++)
            {
                var newCard = GameObject.Instantiate(
                    cardPrefab,
                    spawnPosition,
                    Quaternion.identity
                ) as GameObject;
                newCard.transform.SetParent(DeckGO.transform, false);
                newCard.name = "Player " + spawnDeck.playerId + " Card " + c;

                var cardModel = new CardModel()
                {
                    playerId = spawnDeck.playerId,
                    gameObject = newCard
                };

                decks.Cards.Add(cardModel);
            }

            deckSpawned.Dispatch(spawnDeck);
            debug.Log(string.Format("Spawned deck for player {0}", spawnDeck.playerId), socketKey);
        }
    }
}


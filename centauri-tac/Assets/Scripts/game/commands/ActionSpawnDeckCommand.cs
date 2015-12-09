using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;
using System.Linq;
using System;
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

            var DeckGO = GameObject.Find("Deck");

            for (var c = 0; c < spawnDeck.cards; c++)
            {
                var newPiece = GameObject.Instantiate(
                    cardPrefab,
                    spawnPosition,
                    Quaternion.identity
                ) as GameObject;
                newPiece.transform.SetParent(DeckGO.transform, false);
                newPiece.name = "Player " + spawnDeck.playerId + " Card " + c;

                var cardModel = new CardModel()
                {
                    playerId = spawnDeck.playerId,
                    gameObject = newPiece
                };

                decks.Cards.Add(cardModel);
            }

            deckSpawned.Dispatch(spawnDeck);
            debug.Log(string.Format("Spawned deck for player {0}", spawnDeck.playerId), socketKey);
        }
    }
}


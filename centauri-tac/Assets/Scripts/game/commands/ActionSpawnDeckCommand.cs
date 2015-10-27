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
            //check to see if this action has already been processed by another player
            if (processedActions.processedActions.Any(x => x == spawnDeck.id))
            {
                return;
            }
            processedActions.processedActions.Add(spawnDeck.id);

            var DeckGO = GameObject.Find("Deck");

            for (var c = 0; c < spawnDeck.cards; c++)
            {
                var newPiece = GameObject.Instantiate(
                    cardPrefab,
                    spawnPosition,
                    Quaternion.identity
                ) as GameObject;
                newPiece.transform.SetParent(DeckGO.transform, false);

                var cardModel = new CardModel()
                {
                    playerId = spawnDeck.playerId,
                    gameObject = newPiece
                };

                decks.Cards.Add(cardModel);
            }

            debug.Log(string.Format("Spawned deck for player {0}", spawnDeck.playerId), socketKey);
        }
    }
}


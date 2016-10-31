using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;

namespace ctac
{
    public class ActionSpawnDeckCommand : Command
    {
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
        public ICardService cardService { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }
        
        public override void Execute()
        {
            if (!processedActions.Verify(spawnDeck.id)) return;

            //holding area that all cards go into to start with until the deck mediator sorts them out
            var DeckGO = GameObject.Find("Deck");

            for (var c = 0; c < spawnDeck.cards; c++)
            {
                var cardModel = new CardModel()
                {
                    playerId = spawnDeck.playerId,
                };

                cardService.CreateCard(cardModel, DeckGO.transform);

                decks.Cards.Add(cardModel);
            }

            deckSpawned.Dispatch(spawnDeck);
            debug.Log(string.Format("Spawned deck for player {0}", spawnDeck.playerId), socketKey);
        }
    }
}


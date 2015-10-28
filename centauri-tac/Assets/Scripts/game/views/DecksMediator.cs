using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class DecksMediator : Mediator
    {
        [Inject]
        public DecksView view { get; set; }

        [Inject]
        public DecksModel decks { get; set; }

        [Inject]
        public DeckSpawnedSignal deckSpawned { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        private List<int> playersInitted = new List<int>();

        public override void OnRegister()
        {
            turnEnded.AddListener(onTurnEnded);
            deckSpawned.AddListener(onDeckSpawned);
        }

        public override void onRemove()
        {
            base.onRemove();
            turnEnded.RemoveListener(onTurnEnded);
            deckSpawned.RemoveListener(onDeckSpawned);
        }

        private void onDeckSpawned(SpawnDeckModel deck)
        {
            initPlayerDeck(deck.playerId);
        }

        private void onTurnEnded()
        {
            initPlayerDeck(gameTurn.currentPlayerId);
        }

        private void initPlayerDeck(int playerId)
        {
            var cards = GetPlayerCards(playerId);
            if (!playersInitted.Contains(playerId))
            {
                view.init(cards);
                playersInitted.Add(playerId);
            }
        }

        private List<CardModel> GetPlayerCards(int playerId)
        {
            if(decks == null || decks.Cards == null) return new List<CardModel>();
            //hide non player cards
            var nonPlayerCards = decks.Cards.Where(c => c.playerId != playerId).ToList();
            foreach (var card in nonPlayerCards)
            {
                card.gameObject.SetActive(false);
            }

            //enable player cards
            var playerCards = decks.Cards.Where(c => c.playerId == playerId).ToList();
            foreach (var card in playerCards)
            {
                card.gameObject.SetActive(true);
            }

            return playerCards;
        }
    }
}


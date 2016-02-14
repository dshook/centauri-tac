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
            //wait till the turn ends so we can sort through the cards
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            updateDecks();
        }

        private void updateDecks()
        {
            var playerCards = GetPlayerCards();
            var opponentCards = GetOpponentCards();
            view.UpdatePositions(playerCards, opponentCards);
        }

        private List<CardModel> GetPlayerCards()
        {
            if(decks == null || decks.Cards == null) return new List<CardModel>();
            return decks.Cards.Where(c => c.playerId == gameTurn.currentPlayerId).ToList();
        }

        private List<CardModel> GetOpponentCards()
        {
            if(decks == null || decks.Cards == null) return new List<CardModel>();
            return decks.Cards.Where(c => c.playerId != gameTurn.currentPlayerId).ToList();
        }
    }
}


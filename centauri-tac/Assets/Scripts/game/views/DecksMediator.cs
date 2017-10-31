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
        public ShuffleToDeckSignal shuffleToDeck { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        [Inject] public GameTurnModel gameTurn { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        public override void OnRegister()
        {
            turnEnded.AddListener(onTurnEnded);
            deckSpawned.AddListener(onDeckSpawned);
            shuffleToDeck.AddListener(onShuffleToDeck);
        }

        public override void OnRemove()
        {
            turnEnded.RemoveListener(onTurnEnded);
            deckSpawned.RemoveListener(onDeckSpawned);
            shuffleToDeck.RemoveListener(onShuffleToDeck);
        }

        private void onDeckSpawned(SpawnDeckModel deck)
        {
            updateDecks();
        }

        private void onShuffleToDeck(CardModel card)
        {
            //TODO: Better animation
            updateDecks();
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

        private List<CardModel> emptyList = new List<CardModel>();

        private List<CardModel> GetPlayerCards()
        {
            if(decks == null || decks.Cards == null) return emptyList;
            return decks.Cards
                .Where(c => c.playerId == players.Me.id)
                .ToList();
        }

        private List<CardModel> GetOpponentCards()
        {
            if(decks == null || decks.Cards == null) return emptyList;
            return decks.Cards
                .Where(c => c.playerId != players.Me.id)
                .ToList();
        }
    }
}


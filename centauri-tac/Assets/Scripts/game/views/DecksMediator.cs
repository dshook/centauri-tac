using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class DecksMediator : Mediator
    {
        [Inject] public DecksView view { get; set; }

        [Inject] public DecksModel decks { get; set; }
        [Inject] public GameTurnModel gameTurn { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        [ListensTo(typeof(DeckSpawnedSignal))]
        public void onDeckSpawned(SpawnDeckModel deck)
        {
            updateDecks();
        }

        [ListensTo(typeof(ShuffleToDeckSignal))]
        public void onShuffleToDeck(CardModel card)
        {
            //TODO: Better animation
            updateDecks();
        }

        [ListensTo(typeof(TurnEndedSignal))]
        public void onTurnEnded(GameTurnModel turns)
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


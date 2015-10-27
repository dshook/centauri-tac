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
        public TurnEndedSignal turnEnded { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        private List<int> playersInitted = new List<int>();

        public override void OnRegister()
        {
            view.init(GetCurrentPlayerCards());
            turnEnded.AddListener(onTurnEnded);
        }

        public override void onRemove()
        {
            base.onRemove();
            turnEnded.RemoveListener(onTurnEnded);
        }

        private void onTurnEnded()
        {
            var cards = GetCurrentPlayerCards();
            var player = gameTurn.currentPlayerId;
            if (!playersInitted.Contains(player))
            {
                view.init(cards);
                playersInitted.Add(player);
            }
        }

        private List<CardModel> GetCurrentPlayerCards()
        {
            if(decks == null || decks.Cards == null) return new List<CardModel>();
            //hide non player cards
            var nonPlayerCards = decks.Cards.Where(c => c.playerId != gameTurn.currentPlayerId).ToList();
            foreach (var card in nonPlayerCards)
            {
                card.gameObject.SetActive(false);
            }

            //enable player cards
            var playerCards = decks.Cards.Where(c => c.playerId == gameTurn.currentPlayerId).ToList();
            foreach (var card in playerCards)
            {
                card.gameObject.SetActive(true);
            }

            return playerCards;
        }
    }
}


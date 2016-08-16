using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class EndTurnMediator : Mediator
    {
        [Inject] public EndTurnView view { get; set; }

        [Inject] public EndTurnSignal endTurnSignal { get; set; }

        [Inject] public GameTurnModel gameTurn { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public CardsModel cards { get; set; }

        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public ISocketService socket { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onTurnClicked);
            turnEnded.AddListener(onTurnEnded);
            view.init();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onTurnClicked);
            turnEnded.RemoveListener(onTurnEnded);
        }

        public void Update()
        {
            if (cards.Cards.Count > 0)
            {
                view.updatePlayable(
                    cards.Cards.Any(x => x.playerId == gameTurn.currentPlayerId && x.playable)
                );
            }
        }

        private void onTurnClicked()
        {
            socket.Request(gameTurn.currentTurnClientId, "game", "endTurn");
            endTurnSignal.Dispatch();
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            var opponentId = players.OpponentId(gameTurn.currentPlayerId);

            var text = "End Turn";
            if (gameTurn.currentPlayerId == opponentId)
            {
                text = "Enemy Turn";
            }
            view.onTurnEnded(text);
        }

    }
}


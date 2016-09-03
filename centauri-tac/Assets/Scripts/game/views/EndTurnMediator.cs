using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using System.Collections;
using UnityEngine;

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
        [Inject] public StartGameSettledSignal onStartSettled { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        bool startSettled = false;

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onTurnClicked);
            turnEnded.AddListener(onTurnEnded);
            onStartSettled.AddListener(onStartSet);
            view.init();

            //Really ghetto way to delay the button looking for updates
            StartCoroutine(WaitAndStart(10.0f));
        }

        IEnumerator WaitAndStart(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            debug.Log("Start settled");
            onStartSettled.Dispatch();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onTurnClicked);
            turnEnded.RemoveListener(onTurnEnded);
            onStartSettled.RemoveListener(onStartSet);
        }

        public void Update()
        {
            if (startSettled)
            {
                var opponentId = players.OpponentId(gameTurn.currentPlayerId);
                view.updatePlayable(
                    gameTurn.currentPlayerId != opponentId &&
                    cards.Cards.Any(x => x.playerId != opponentId && x.playable)
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

        private void onStartSet()
        {
            startSettled = true;
        }
    }
}


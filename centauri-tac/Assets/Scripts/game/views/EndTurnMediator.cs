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
            view.clickEndTurnSignal.AddListener(onTurnClicked);
            view.clickSwitchSidesSignal.AddListener(onSwitchClicked);
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
            view.clickEndTurnSignal.RemoveListener(onTurnClicked);
            turnEnded.RemoveListener(onTurnEnded);
            onStartSettled.RemoveListener(onStartSet);
        }

        public void Update()
        {
            if (startSettled)
            {
                view.updatePlayable(
                    cards.Cards.Any(x => x.playerId == players.Me.id && x.playable)
                );
            }
        }

        private void onTurnClicked()
        {
            socket.Request(players.Me.clientId, "game", "endTurn");
            endTurnSignal.Dispatch();
        }

        private void onSwitchClicked()
        {
            var opponent = players.Opponent(players.Me.id);
            players.SetMeClient(opponent.clientId);
            turnEnded.Dispatch(new GameTurnModel() { currentTurn = gameTurn.currentTurn, isClientSwitch = true });
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            var text = "End Turn";
            view.onTurnEnded(text);
        }

        private void onStartSet()
        {
            startSettled = true;
        }
    }
}


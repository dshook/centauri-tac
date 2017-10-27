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

        [Inject] public GamePausedSignal pauseSignal { get; set; }
        [Inject] public GameResumedSignal resumeSignal { get; set; }

        [Inject] public GameTurnModel gameTurn { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public PiecesModel pieces { get; set; }

        [Inject] public TurnEndedSignal turnEnded { get; set; }
        [Inject] public ActionKickoffSignal kickoff { get; set; }
        [Inject] public MoveCameraToTileSignal moveCamSignal { get; set; }


        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        bool startSettled = false;

        public override void OnRegister()
        {
            view.clickEndTurnSignal.AddListener(onTurnClicked);
            view.clickSwitchSidesSignal.AddListener(onSwitchClicked);
            view.clickPauseSignal.AddListener(onPauseClicked);
            view.clickResumeSignal.AddListener(onResumeClicked);
            turnEnded.AddListener(onTurnEnded);
            kickoff.AddListener(onStartSet);
            view.init();
        }

        public override void onRemove()
        {
            view.clickEndTurnSignal.RemoveListener(onTurnClicked);
            turnEnded.RemoveListener(onTurnEnded);
            kickoff.RemoveListener(onStartSet);
        }

        public void Update()
        {
            if (!startSettled) { return; }

            view.updatePlayable(
                cards.Cards.Any(x => x.playerId == players.Me.id && x.playable)
            );
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
            var newHero = pieces.Hero(opponent.id);
            moveCamSignal.Dispatch(newHero.tilePosition);
        }

        private void onPauseClicked()
        {
            socket.Request(players.Me.clientId, "game", "pauseGame");
            pauseSignal.Dispatch();
        }

        private void onResumeClicked()
        {
            socket.Request(players.Me.clientId, "game", "resumeGame");
            resumeSignal.Dispatch();
        }

        private void onTurnEnded(GameTurnModel turns)
        {
            var text = "End Turn";
            view.onTurnEnded(text);
        }

        private void onStartSet(KickoffModel km, SocketKey key)
        {
            startSettled = true;
        }
    }
}


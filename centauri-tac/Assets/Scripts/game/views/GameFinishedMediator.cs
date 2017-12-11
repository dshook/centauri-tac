using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class GameFinishedMediator : Mediator
    {
        [Inject] public GameFinishedView view { get; set; }

        [Inject] public GameFinishedSignal gameFinished { get; set; }
        [Inject] public LeaveGameSignal leaveSignal { get; set; }

        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public GameInputStatusModel gameInputStatus { get; set; }

        [Inject] public IDebugService debug { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLeaveClicked);

            view.init();
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onLeaveClicked);
        }

        [ListensTo(typeof(GameFinishedSignal))]
        public void onGameFinished(GameFinishedModel gameFinished)
        {
            gameInputStatus.inputEnabled = false;

            view.onFinish(gameFinished.message);
        }

        private void onLeaveClicked()
        {
            leaveSignal.Dispatch(new SocketKey(players.Me.clientId, "game"), true);
        }

        [ListensTo(typeof(SocketHangupSignal))]
        public void onSocketDisconnect(SocketKey key)
        {
            //For now game is over on DC
            debug.Log("Finishing game from disconnect");
            gameFinished.Dispatch(new GameFinishedModel() { id = 9999, winnerId = -1, message = "Disconnected from server :(", isDisconnect = true });
        }
    }
}


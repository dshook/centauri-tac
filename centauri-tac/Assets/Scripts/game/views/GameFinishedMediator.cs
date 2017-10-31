using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class GameFinishedMediator : Mediator
    {
        [Inject] public GameFinishedView view { get; set; }

        [Inject] public GameFinishedSignal gameFinishedSignal { get; set; }
        [Inject] public LeaveGameSignal leaveSignal { get; set; }

        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public GameInputStatusModel gameInputStatus { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLeaveClicked);
            gameFinishedSignal.AddListener(onGameFinished);

            view.init();
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onLeaveClicked);
        }

        private void onGameFinished(GameFinishedModel gameFinished)
        {
            gameInputStatus.inputEnabled = false;

            if (gameFinished.winnerId == players.Me.id)
            {
                view.onFinish("Victory!");
            }
            else
            {
                view.onFinish("Defeat :(");
            }
        }

        private void onLeaveClicked()
        {
            leaveSignal.Dispatch(new SocketKey(players.Me.clientId, "game"), true);
        }

    }
}


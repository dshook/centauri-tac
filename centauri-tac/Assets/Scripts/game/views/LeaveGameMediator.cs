using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class LeaveGameMediator : Mediator
    {
        [Inject]
        public LeaveGameView view { get; set; }

        [Inject]
        public LeaveGameSignal leaveSignal { get; set; }

        [Inject]
        public GamePlayersModel players { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onLeaveClicked);
            view.init();
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onLeaveClicked);
        }

        private void onLeaveClicked(bool returnToMainMenu)
        {
            leaveSignal.Dispatch(new SocketKey(players.Me.clientId, "game"), returnToMainMenu);
        }

    }
}


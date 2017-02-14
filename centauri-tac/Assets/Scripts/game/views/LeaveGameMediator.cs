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
            view.clickSignal.AddListener(onTurnClicked);
            view.init();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onTurnClicked);
        }

        private void onTurnClicked()
        {
            leaveSignal.Dispatch(new SocketKey(players.Me.clientId, "game"));
        }

    }
}


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
            returnToMainMenu = true;
#if UNITY_EDITOR
            returnToMainMenu = false;
#endif
            leaveSignal.Dispatch(returnToMainMenu);
        }

    }
}


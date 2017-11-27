using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class LoginWatcherMediator : Mediator
    {
        [Inject] public LoginWatcherView view { get; set; }

        [Inject] public MatchmakerQueueSignal matchmakerQueue { get; set; }

        public override void OnRegister()
        {
        }

        public override void OnRemove()
        {
        }


        [ListensTo(typeof(LobbyLoggedInSignal))]
        public void onLobbyLogin(LoginStatusModel lsm, SocketKey key)
        {
            matchmakerQueue.Dispatch(new QueueModel(), key);
        }
    }
}


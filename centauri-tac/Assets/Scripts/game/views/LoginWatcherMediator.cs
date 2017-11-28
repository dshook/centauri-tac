using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class LoginWatcherMediator : Mediator
    {
        [Inject] public LoginWatcherView view { get; set; }

        [Inject] public MatchmakerQueueSignal matchmakerQueue { get; set; }


        [ListensTo(typeof(LobbyLoggedInSignal))]
        public void onLobbyLogin(LoginStatusModel lsm, SocketKey key)
        {
            //Auto queue to matchmaker in dev 
            matchmakerQueue.Dispatch(new QueueModel(), key);
        }
    }
}


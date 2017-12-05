using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class AuthLoggedInCommand : Command
    {
        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public LoginStatusModel loginStatus { get; set; }

        [Inject]
        public SocketKey loggedInKey { get; set; }

        [Inject]
        public NeedLoginSignal needLogin { get; set; }

        public override void Execute()
        {
            if (loginStatus.status)
            {
                socketService.Request(loggedInKey.clientId, "auth", "me");
            }
            else
            {
                needLogin.Dispatch(null);
            }
        }
    }
}


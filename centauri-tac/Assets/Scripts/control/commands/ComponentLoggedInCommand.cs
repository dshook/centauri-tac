using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class ComponentLoggedInCommand : Command
    {
        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public AuthLoggedInSignal authLoggedIn { get; set; }

        [Inject]
        public GameLoggedInSignal gameLoggedIn { get; set; }

        [Inject]
        public LobbyLoggedInSignal lobbyLoggedIn { get; set; }

        [Inject]
        public LoginStatusModel status { get; set; }

        [Inject]
        public SocketKey loggedInKey { get; set; }

        public override void Execute()
        {
            switch (loggedInKey.componentName) {
                case "auth":
                    authLoggedIn.Dispatch(status, loggedInKey);
                    break;
                case "game":
                    gameLoggedIn.Dispatch(status, loggedInKey);
                    break;
                case "lobby":
                    lobbyLoggedIn.Dispatch(status, loggedInKey);
                    break;
            }
        }
    }
}


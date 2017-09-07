using strange.extensions.command.impl;

namespace ctac
{
    public class AuthLobbyCommand : Command
    {
        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public PlayerModel playerModel { get; set; } 

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            socketService.Request(key.clientId, "lobby", "token", playerModel.token);
        }

    }
}


using strange.extensions.command.impl;

namespace ctac
{
    public class AuthMatchmakerCommand : Command
    {
        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public PlayerModel playerModel { get; set; } 

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            socketService.Request(key.clientId, "matchmaker", "token", playerModel.token);
        }

    }
}


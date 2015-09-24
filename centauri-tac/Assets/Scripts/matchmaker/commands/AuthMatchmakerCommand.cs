using strange.extensions.command.impl;

namespace ctac
{
    public class AuthMatchmakerCommand : Command
    {
        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public PlayersModel playersModel { get; set; } 

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            var player = playersModel.GetByClientId(key.clientId);
            socketService.Request(key.clientId, "matchmaker", "token", player.token);
        }

    }
}


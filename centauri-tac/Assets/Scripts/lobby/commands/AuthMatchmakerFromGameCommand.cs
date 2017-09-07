using strange.extensions.command.impl;

namespace ctac
{
    public class AuthLobbyFromGameCommand : Command
    {
        [Inject]
        public ISocketService socketService { get; set; }

        [Inject] public PlayersModel playersModel { get; set; } 

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            string token = playersModel.GetByClientId(key.clientId).token;
            socketService.Request(key.clientId, "lobby", "token", token);
        }

    }
}


using strange.extensions.command.impl;

namespace ctac
{
    public class GetDecksCommand : Command
    {
        [Inject] public IDebugService debug { get; set; }
        [Inject] public ISocketService socket { get; set; }

        [Inject] public LobbyModel lobbyModel { get; set; }

        public override void Execute()
        {
            if (lobbyModel.lobbyKey == null)
            {
                debug.LogError("Can't get decks without a valid lobby key");
                return;
            }
            socket.Request(lobbyModel.lobbyKey, "getDecks");
        }
    }
}


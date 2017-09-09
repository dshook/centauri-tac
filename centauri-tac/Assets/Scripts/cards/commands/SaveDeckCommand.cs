using strange.extensions.command.impl;

namespace ctac
{
    public class SaveDeckCommand : Command
    {
        [Inject] public IDebugService debug { get; set; }
        [Inject] public ISocketService socket { get; set; }

        [Inject] public LobbyModel lobbyModel { get; set; }

        [Inject] public DeckModel deck { get; set; }

        public override void Execute()
        {
            if (lobbyModel.lobbyKey == null)
            {
                debug.LogError("Can't save deck without a valid lobby key");
                return;
            }
            socket.Request(lobbyModel.lobbyKey, "saveDeck", deck);
        }
    }
}


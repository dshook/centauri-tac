using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class JoinGameCommand : Command
    {
        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public GameAuthedSignal gameAuthed { get; set; }

        [Inject] public LoginStatusModel loginModel { get; set; }
        [Inject] public SocketKey socketKey { get; set; }
        [Inject] public GameMetaModel gameToJoin { get; set; }

        public override void Execute()
        {
            if (!loginModel.status)
            {
                debug.LogWarning("Could not log into game, bailing for now: " + loginModel.status); 
                return;
            }
            if (gameToJoin != null)
            {
                debug.Log("Joining game " + gameToJoin.id, socketKey);
                socket.Request(socketKey, "join", gameToJoin.id);
                gameAuthed.Dispatch();
            }
        }
    }
}


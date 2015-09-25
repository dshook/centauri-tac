using strange.extensions.command.impl;

namespace ctac
{
    public class StartGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            debug.Log("Game Started", socketKey);
            
            gameTurn.currentTurnClientId = socketKey.clientId;
        }
    }
}


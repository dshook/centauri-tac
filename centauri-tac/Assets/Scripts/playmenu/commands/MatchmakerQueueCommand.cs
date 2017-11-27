using strange.extensions.command.impl;

namespace ctac
{
    public class MatchmakerQueueCommand : Command
    {
        [Inject] public ISocketService socketService { get; set; }

        [Inject] public IDebugService debug { get; set; }

        [Inject] public SocketKey key { get; set; }

        public override void Execute()
        {
            debug.Log("Player Queuing", key);
            socketService.Request(key, "queue");
        }

    }
}


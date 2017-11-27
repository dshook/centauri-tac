using strange.extensions.command.impl;

namespace ctac
{
    public class MatchmakerDequeueCommand : Command
    {
        [Inject] public ISocketService socketService { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public SocketKey key { get; set; }

        public override void Execute()
        {
            debug.Log("Player Dequeuing", key);
            socketService.Request(key, "dequeue");
        }

    }
}


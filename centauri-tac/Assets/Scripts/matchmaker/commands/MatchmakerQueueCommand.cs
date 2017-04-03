using strange.extensions.command.impl;

namespace ctac
{
    public class MatchmakerQueueCommand : Command
    {
        [Inject] public ISocketService socketService { get; set; }

        [Inject] public IDebugService debug { get; set; }

        [Inject] public LoginStatusModel loginStatus { get; set; }
        [Inject] public SocketKey key { get; set; }

        public override void Execute()
        {
            if (loginStatus.status == false)
            {
                debug.LogError("Could not log into matchmaker service: " + loginStatus.message);
                return;
            }
            socketService.Request(key, "queue");
        }

    }
}


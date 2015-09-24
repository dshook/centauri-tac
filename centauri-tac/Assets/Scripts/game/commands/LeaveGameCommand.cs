using strange.extensions.command.impl;

namespace ctac
{
    public class LeaveGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            socket.Request(socketKey, "part");
        }
    }
}


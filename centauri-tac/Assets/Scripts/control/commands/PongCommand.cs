using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class PongCommand : Command
    {

        [Inject]
        public PingSignal ping { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public int id { get; set; }

        [Inject]
        public SocketKey key { get; set; }

        public override void Execute()
        {
            socket.Request(key.clientId, key.componentName, "_pong", id);
        }
    }
}


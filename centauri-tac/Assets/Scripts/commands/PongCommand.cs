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

        public override void Execute()
        {
            var dataArray = (object[])data;
            var id = (int)dataArray[0];
            var key  = dataArray[1] as SocketKey;
            //TODO: make sure the pong is sent back on the right socket
            socket.Request(key.clientId, key.componentName, "_pong", id);
        }
    }
}


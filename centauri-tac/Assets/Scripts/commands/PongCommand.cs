using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class PongCommand : Command
    {

        [Inject]
        public PingSignal ping { get; set; }

        [Inject(name = InjectKeys.authSocketService)]
        public ISocketService socket { get; set; }

        public override void Execute()
        {
            var dataArray = (object[])data;
            var id = (int)dataArray[0];
            string componentName = (string)dataArray[1];
            //TODO: make sure the pong is sent back on the right socket
            socket.Request(componentName, "_pong", id);
        }
    }
}


using strange.extensions.command.impl;
using System;

namespace ctac
{
    public class GamelistCreateGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public LoginStatusModel status { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            debug.Log("Creating game", socketKey);
            socket.Request(socketKey, "create", new
            {
                name = socketKey.clientId
            });
        }
    }
}


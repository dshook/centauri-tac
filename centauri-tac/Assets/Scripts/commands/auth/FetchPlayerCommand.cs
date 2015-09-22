/// The only change in StartCommand is that we extend Command, not EventCommand
using UnityEngine;
using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class FetchPlayerCommand : Command
    {
        [Inject]
        public PlayersModel playersModel { get; set; }

        [Inject]
        public ISocketService socketService { get; set; }

        [Inject]
        public SocketKey loggedInKey { get; set; }

        public override void Execute()
        {
            if (loggedInKey.componentName == "auth")
            {
                socketService.Request(loggedInKey.clientId, "auth", "me");
            }
        }
    }
}


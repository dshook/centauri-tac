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

        public override void Execute()
        {
            SocketKey loggedInKey = ((object[])data)[1] as SocketKey;
            socketService.Request(loggedInKey.clientId, "auth", "me");
        }
    }
}


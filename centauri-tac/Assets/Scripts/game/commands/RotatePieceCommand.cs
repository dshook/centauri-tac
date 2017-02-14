using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class RotatePieceCommand : Command
    {
        [Inject]
        public RotatePieceModel rotateModel { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GamePlayersModel players { get; set; }

        public override void Execute()
        {
            socket.Request(players.Me.clientId, "game", "rotate", rotateModel );
        }
    }
}


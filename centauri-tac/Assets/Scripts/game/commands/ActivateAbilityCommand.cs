using strange.extensions.command.impl;

namespace ctac
{
    public class ActivateAbilityCommand : Command
    {
        [Inject]
        public ActivateAbilityModel activatedAbility { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GamePlayersModel players { get; set; }

        public override void Execute()
        {
            socket.Request(players.Me.clientId, "game", "activateability", 
                new { pieceId = activatedAbility.piece.id, targetPieceId = activatedAbility.optionalTarget == null ? (int?)null : activatedAbility.optionalTarget.id }
            );
        }
    }
}


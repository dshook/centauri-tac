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
        public GameTurnModel gameTurn { get; set; }


        public override void Execute()
        {
            socket.Request(gameTurn.currentTurnClientId, "game", "activateability", 
                new { pieceId = activatedAbility.piece.id, targetPieceId = activatedAbility.optionalTarget == null ? (int?)null : activatedAbility.optionalTarget.id }
            );
        }
    }
}


using strange.extensions.command.impl;

namespace ctac
{
    public class ActivateModel
    {
        public CardModel cardActivated { get; set; }
        public Tile tilePlayedAt { get; set; }
        public PieceModel optionalTarget { get; set; }
    }

    public class ActivateCardCommand : Command
    {
        [Inject]
        public ActivateModel cActivate { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        public override void Execute()
        {
            socket.Request(gameTurn.currentTurnClientId, "game", "activatecard", 
                new ActivateCardModel {
                    playerId = cActivate.cardActivated.playerId,
                    cardInstanceId = cActivate.cardActivated.id,
                    position = cActivate.tilePlayedAt.position.ToPositionModel(),
                    targetPieceId = cActivate.optionalTarget != null ? cActivate.optionalTarget.id : (int?)null
                }
            );
        }
    }
}


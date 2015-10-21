using strange.extensions.command.impl;
using System.Linq;
using System.Collections.Generic;

namespace ctac
{
    public class ActivateCardCommand : Command
    {
        [Inject]
        public CardModel cardActivated { get; set; }

        [Inject]
        public Tile tilePlayedAt { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        public override void Execute()
        {
            socket.Request(gameTurn.currentTurnClientId, "game", "activatecard", 
                new { cardID = cardActivated.id, tile = tilePlayedAt.position.ToPositionModel() }
            );
        }
    }
}


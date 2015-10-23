using strange.extensions.command.impl;
using System.Linq;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public class ActivateCardCommand : Command
    {
        [Inject]
        public CardModel cardActivated { get; set; }

        [Inject]
        public DestroyCardSignal destroyCard { get; set; }

        [Inject]
        public Tile tilePlayedAt { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GameTurnModel gameTurn { get; set; }

        public override void Execute()
        {
            socket.Request(gameTurn.currentTurnClientId, "game", "activatecard", 
                new { playerId = cardActivated.playerId, cardID = cardActivated.id, position = tilePlayedAt.position.ToPositionModel() }
            );
            destroyCard.Dispatch(cardActivated);
        }
    }
}


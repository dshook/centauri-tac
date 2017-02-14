using strange.extensions.command.impl;
using UnityEngine;

namespace ctac
{
    public class ActivateModel
    {
        public CardModel cardActivated { get; set; }
        public PieceModel optionalTarget { get; set; }
        public Vector2? position { get; set; }
        public Vector2? pivotPosition { get; set; }
        public int? chooseCardTemplateId { get; set; }
    }

    public class ActivateCardCommand : Command
    {
        [Inject]
        public ActivateModel cActivate { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public GamePlayersModel players { get; set; }

        public override void Execute()
        {
            socket.Request(players.Me.clientId, "game", "activatecard", 
                new ActivateCardModel {
                    playerId = cActivate.cardActivated.playerId,
                    cardInstanceId = cActivate.cardActivated.id,
                    position = cActivate.position == null ? null : cActivate.position.Value.ToPositionModel(),
                    pivotPosition = cActivate.pivotPosition == null ? null : cActivate.pivotPosition.Value.ToPositionModel(),
                    targetPieceId = cActivate.optionalTarget != null ? cActivate.optionalTarget.id : (int?)null,
                    chooseCardTemplateId = cActivate.chooseCardTemplateId
                }
            );
        }
    }
}


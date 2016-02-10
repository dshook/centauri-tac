using strange.extensions.command.impl;
using System.Collections.Generic;

namespace ctac
{
    public class StartSelectTargetCommand : Command
    {
        [Inject]
        public CardModel targetingCard { get; set; }

        [Inject]
        public Tile spawnPosition { get; set; }

        [Inject]
        public ActionTarget actionTargets { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public IPieceService pieceService { get; set; }

        public override void Execute()
        {
            if(targetingCard.tags.Contains("Spell")) return;

            var spawnedPiece = new SpawnPieceModel
            {
                cardTemplateId = targetingCard.cardTemplateId,
                pieceId = -1,
                playerId = targetingCard.playerId,
                position = spawnPosition.position.ToPositionModel(),
                tags = new List<string>() { "targetPiece" }
            };

            pieceService.CreatePiece(spawnedPiece);

            //Skip sending out the piece spawned signal since it hasn't actually properly been spawned yet
            //pieceSpawned.Dispatch(pieceModel);
            debug.Log("Spawned phantom target piece");
        }
    }
}


using strange.extensions.command.impl;
using System.Collections.Generic;

namespace ctac
{
    public class StartSelectTargetCommand : Command
    {
        [Inject]
        public StartTargetModel startTargetModel { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public IPieceService pieceService { get; set; }
        [Inject] public IMapService mapService { get; set; }

        [Inject]
        public AnimationQueueModel animationQueue { get; set; }
        [Inject]
        public MapModel map { get; set; }

        public override void Execute()
        {
            if(startTargetModel.targetingCard.tags.Contains("Spell")) return;

            var spawnedPiece = new SpawnPieceModel
            {
                cardTemplateId = startTargetModel.targetingCard.cardTemplateId,
                pieceId = -1,
                playerId = startTargetModel.targetingCard.playerId,
                position = startTargetModel.cardDeployPosition.position.ToPositionModel(),
                tags = new List<string>() { Constants.targetPieceTag },
                direction = Direction.South
            };

            var pieceModel = pieceService.CreatePiece(spawnedPiece);
            animationQueue.Add(new PieceView.SpawnAnim() {
                piece = pieceModel.gameObject.GetComponent<PieceView>(),
                map = map,
                mapService = mapService
            });

            //Skip sending out the piece spawned signal since it hasn't actually properly been spawned yet
            //pieceSpawned.Dispatch(pieceModel);
            debug.Log("Spawned phantom target piece");
        }
    }
}


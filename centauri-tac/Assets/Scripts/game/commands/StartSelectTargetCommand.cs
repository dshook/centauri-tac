using strange.extensions.command.impl;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class StartSelectTargetCommand : Command
    {
        [Inject] public TargetModel startTargetModel { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public IPieceService pieceService { get; set; }
        [Inject] public IMapService mapService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }
        [Inject] public ISoundService sounds { get; set; }

        [Inject] public AnimationQueueModel animationQueue { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public TraumaModel trauma { get; set; }
        [Inject] public PiecesModel pieces { get; set; }

        public override void Execute()
        {
            if(startTargetModel.targetingCard.isSpell) return;

            var phantomPiece = pieces.Pieces.FirstOrDefault(p => p.tags.Contains(Constants.targetPieceTag));

            if (phantomPiece != null)
            {
                //skip spawning if there's already the phantom piece out, like in a choose action
                debug.Log("Phantom piece already spawned");
                return;
            }

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
            var pieceView = pieceModel.gameObject.GetComponent<PieceView>();
            animationQueue.Add(new PieceView.SpawnAnim() {
                piece = pieceView,
                map = map,
                trauma = trauma,
                sounds = sounds,
                mapService = mapService,
                loader = loader
            });
            animationQueue.Add(
                new PieceView.ChangeStatusAnim()
                {
                    piece = pieceView,
                    loader = loader,
                    pieceStatusChange = new PieceStatusChangeModel() { add = pieceModel.statuses, statuses = pieceModel.statuses }
                }
            );

            //Skip sending out the piece spawned signal since it hasn't actually properly been spawned yet
            //pieceSpawned.Dispatch(pieceModel);
            debug.Log("Spawned phantom target piece");
        }
    }
}


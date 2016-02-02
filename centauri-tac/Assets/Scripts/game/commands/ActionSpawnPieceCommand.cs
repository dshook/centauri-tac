using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionSpawnPieceCommand : Command
    {
        [Inject]
        public SpawnPieceModel spawnedPiece { get; set; }

        [Inject]
        public PieceSpawnedSignal pieceSpawned { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public MapModel map { get; set; }

        [Inject]
        public IPieceService pieceService { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(spawnedPiece.id)) return;

            //search for phantom piece that was created when targeting started to update props from real spawn event
            var phantomPiece = pieces.Pieces.FirstOrDefault(p => p.id == -1 && p.tags.Contains("targetPiece"));

            PieceModel pieceModel = null;

            if (phantomPiece != null)
            {
                //update props from real spawn event
                pieceModel = phantomPiece;

                //should be a NOOP but just in case...
                pieceModel.gameObject.transform.position = map.tiles[spawnedPiece.position.Vector2].fullPosition;
                pieceModel.id = spawnedPiece.pieceId;
                pieceModel.playerId = spawnedPiece.playerId;
                pieceModel.cardTemplateId = spawnedPiece.cardTemplateId;
                pieceModel.tags = spawnedPiece.tags;

            }
            else
            {
                pieceModel = pieceService.CreatePiece(spawnedPiece);
            }

            pieceSpawned.Dispatch(pieceModel);
            debug.Log(string.Format("Spawned piece {0} for player {1}", spawnedPiece.cardTemplateId, spawnedPiece.playerId), socketKey);
        }
    }
}


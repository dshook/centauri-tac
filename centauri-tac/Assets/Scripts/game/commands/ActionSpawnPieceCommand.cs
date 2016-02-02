using strange.extensions.command.impl;
using ctac.signals;

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
        public IPieceService pieceService { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(spawnedPiece.id)) return;

            var pieceModel = pieceService.CreatePiece(spawnedPiece);

            pieceSpawned.Dispatch(pieceModel);
            debug.Log(string.Format("Spawned piece {0} for player {1}", spawnedPiece.cardTemplateId, spawnedPiece.playerId), socketKey);
        }
    }
}


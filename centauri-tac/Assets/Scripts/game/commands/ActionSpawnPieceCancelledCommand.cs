using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class ActionSpawnPieceCancelledCommand : Command
    {
        [Inject]
        public SpawnPieceModel pieceSpawnCancelled { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            //find and cleanup the phantom piece start target spawned
            var phantomPiece = pieces.Pieces.FirstOrDefault(p =>  p.tags.Contains(Constants.targetPieceTag));

            if (phantomPiece != null)
            {
                pieceDied.Dispatch(phantomPiece);
            }
        }
    }
}


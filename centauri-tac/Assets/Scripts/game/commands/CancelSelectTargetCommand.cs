using ctac.signals;
using strange.extensions.command.impl;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class CancelSelectTargetCommand : Command
    {
        [Inject]
        public CardModel targetingCard { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            //find and cleanup the phantom piece start target spawned
            var phantomPiece = pieces.Pieces.FirstOrDefault(p => p.id == -1 && p.tags.Contains("targetPiece"));

            if (phantomPiece != null)
            {
                pieceDied.Dispatch(phantomPiece);
            }
            else
            {
                debug.LogWarning("Couldn't find phantom targeting piece to cleanup");
            }
        }
    }
}


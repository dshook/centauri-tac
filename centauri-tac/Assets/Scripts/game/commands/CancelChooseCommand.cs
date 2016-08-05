using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class CancelChooseCommand : Command
    {
        [Inject]
        public ChooseModel chooseModel { get; set; }

        [Inject]
        public PieceDiedSignal pieceDied { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            //find and cleanup the phantom piece from choose if there was one from a minion
            var phantomPiece = pieces.Pieces.FirstOrDefault(p =>  p.tags.Contains(Constants.targetPieceTag));

            if (phantomPiece != null)
            {
                pieceDied.Dispatch(phantomPiece);
            }
        }
    }
}


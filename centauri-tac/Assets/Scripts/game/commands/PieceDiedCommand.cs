using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class PieceDiedCommand : Command
    {
        [Inject]
        public PieceModel pieceDied { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            piecesModel.Pieces.Remove(pieceDied);
            GameObject.Destroy(pieceDied.gameObject, 0.1f);
        }
    }
}


using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class PieceHoverMediator : Mediator
    {
        [Inject]
        public PieceHoverView view { get; set; }
        
        [Inject]
        public PieceHoverSignal pieceHoveredSignal { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public IMapService mapService { get; set; }

        public override void OnRegister()
        {
            view.pieceHover.AddListener(onPieceHover);
            view.init();
        }

        public override void onRemove()
        {
            view.pieceHover.RemoveListener(onPieceHover);
        }

        void onPieceHover(GameObject pieceHovered)
        {
            if (pieceHovered != null)
            {
                var pieceView = pieceHovered.GetComponent<PieceView>();
                pieceHoveredSignal.Dispatch(pieceView.piece);
            }
            else
            {
                pieceHoveredSignal.Dispatch(null);
            }
        }
    }
}


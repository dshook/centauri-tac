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

        public override void OnRegister()
        {
            view.pieceHover.AddListener(onPieceHover);
            view.init();
        }

        public override void onRemove()
        {
            view.pieceHover.RemoveListener(onPieceHover);
        }

        private PieceView lastHoveredPiece = null;
        void onPieceHover(GameObject pieceHovered)
        {
            if (pieceHovered != null)
            {
                var pieceView = pieceHovered.GetComponent<PieceView>();
                if (pieceView != lastHoveredPiece)
                {
                    lastHoveredPiece = pieceView;
                    pieceHoveredSignal.Dispatch(pieceView.piece);
                }
            }
            else
            {
                if (lastHoveredPiece != null)
                {
                    lastHoveredPiece = null;
                    pieceHoveredSignal.Dispatch(null);
                }
            }
        }
    }
}


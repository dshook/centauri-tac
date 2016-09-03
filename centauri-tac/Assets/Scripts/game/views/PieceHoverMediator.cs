using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PieceHoverMediator : Mediator
    {
        [Inject]
        public PieceHoverView view { get; set; }
        
        [Inject] public PieceHoverSignal pieceHoveredSignal { get; set; }

        [Inject] public RaycastModel raycastModel { get; set; }

        public override void OnRegister()
        {
        }

        public override void onRemove()
        {
        }

        void Update()
        {
            onPieceHover(raycastModel.piece);
        }

        private PieceView lastHoveredPiece = null;
        void onPieceHover(PieceView pieceHovered)
        {
            if (pieceHovered != null)
            {
                if (pieceHovered != lastHoveredPiece)
                {
                    lastHoveredPiece = pieceHovered;
                    pieceHoveredSignal.Dispatch(pieceHovered.piece);
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


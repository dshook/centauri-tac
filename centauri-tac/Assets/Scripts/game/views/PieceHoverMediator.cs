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
        [Inject] public PiecesModel pieces { get; set; }

        public override void OnRegister()
        {
        }

        public override void onRemove()
        {
        }

        void Update()
        {
            if (raycastModel.piece)
            {
                onPieceHover(raycastModel.piece);
            }
            else if (raycastModel.tile != null)
            {
                var pieceAtTile = pieces.PieceAt(raycastModel.tile.position);
                if (pieceAtTile != null)
                {
                    onPieceHover(pieceAtTile.pieceView);
                }
            }
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


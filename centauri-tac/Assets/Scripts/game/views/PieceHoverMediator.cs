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

        void Update()
        {
            if (raycastModel.piece != null)
            {
                onPieceHover(raycastModel.piece.piece);
            }
            else if (raycastModel.tile != null)
            {
                var pieceAtTile = pieces.PieceAt(raycastModel.tile.position);
                if (pieceAtTile != null)
                {
                    onPieceHover(pieceAtTile);
                }
                else
                {
                    onPieceHover(null);
                }
            }
            else
            {
                onPieceHover(null);
            }
        }

        private PieceModel lastHoveredPiece = null;
        void onPieceHover(PieceModel pieceHovered)
        {
            if (pieceHovered != lastHoveredPiece)
            {
                lastHoveredPiece = pieceHovered;
                pieceHoveredSignal.Dispatch(pieceHovered);
            }
        }
    }
}


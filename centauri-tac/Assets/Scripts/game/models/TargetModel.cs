using UnityEngine;

namespace ctac
{
    public class TargetModel
    {
        public CardModel targetingCard { get; set; }
        public Tile cardDeployPosition { get; set; }
        public ActionTarget targets { get; set; }
        public AreaTarget area { get; set; }

        public PieceModel selectedPiece { get; set; }
        public Vector2? selectedPosition { get; set; }

        //use pivot position as the 'resolved' position that is the point in the area chosen
        public Vector2? selectedPivotPosition { get; set; }

        public bool targetFulfilled
        {
            get
            {
                if (area != null)
                {
                    if (area.isDoubleCursor)
                    {
                        return selectedPosition.HasValue && selectedPivotPosition.HasValue;
                    }

                    return selectedPosition.HasValue;
                }

                if (targets != null)
                {
                    return selectedPiece != null;
                }
                return false;
            }
        }
    }
}

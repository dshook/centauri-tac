using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public class PieceMovePreviewView : View
    {
        GameObject pieceMovePreview;
        BezierSpline moveSpline;

        internal void init()
        {
            pieceMovePreview = transform.Find("PreviewCurve").gameObject;
            pieceMovePreview.SetActive(false);

            moveSpline = pieceMovePreview.GetComponent<BezierSpline>();
        }

        internal void onMovePath(List<Tile> tiles)
        {
            if (tiles == null || tiles.Count < 2)
            {
                pieceMovePreview.SetActive(false);
                return;
            }

            moveSpline.Reset();

            for (int i = 0; i < tiles.Count; i++)
            {
                var currentTile = tiles[i];
                var nextTile = i + 1 < tiles.Count ? tiles[i + 1] : null;
                var anchorPointIndex = i * 3;

                moveSpline.SetControlPoint(anchorPointIndex, currentTile.fullPosition);
                if (nextTile == null)
                {
                    break;
                }
                if (i > 0)
                {
                    moveSpline.AddCurve();
                }
                var diffVector = nextTile.fullPosition - currentTile.fullPosition;
                var secondControl = (diffVector * 0.2f) + currentTile.fullPosition;
                var thirdControl = (diffVector * 0.8f) + currentTile.fullPosition;

                moveSpline.SetControlPoint(anchorPointIndex + 1, secondControl);
                moveSpline.SetControlPoint(anchorPointIndex + 2, thirdControl);
                //moveSpline.SetControlPoint(i + 3, nextTile.fullPosition);
            }

            pieceMovePreview.SetActive(true);
        }

    }
}


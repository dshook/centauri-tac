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

            var start = tiles[0].fullPosition;
            var destination = tiles[tiles.Count - 1].fullPosition;
            var diffVector = destination - start;
            Vector3 curveHeight = new Vector3(0, 0.30f, 0);
            float curveMult = 1.0f;

            var secondControl = (diffVector * 0.2f) + (curveHeight * diffVector.magnitude * curveMult) + start;
            var thirdControl = (diffVector * 0.8f) + (curveHeight * diffVector.magnitude * curveMult) + start;

            moveSpline.SetControlPoint(0, start);
            moveSpline.SetControlPoint(1, secondControl);
            moveSpline.SetControlPoint(2, thirdControl);
            moveSpline.SetControlPoint(3, destination);

            pieceMovePreview.SetActive(true);
        }

    }
}


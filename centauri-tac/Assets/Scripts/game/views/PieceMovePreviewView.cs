using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public class PieceMovePreviewView : View
    {
        GameObject pieceMovePreview;
        BezierSpline moveSpline;
        SplineDecorator splineDecorator;

        public Color defaultColor;
        public Color attackColor;

        public float height = 0.5f;
        Vector3 curveHeight = new Vector3(0, 0.50f, 0);

        internal void init()
        {
            pieceMovePreview = transform.Find("PreviewCurve").gameObject;
            pieceMovePreview.SetActive(false);

            moveSpline = pieceMovePreview.GetComponent<BezierSpline>();
            splineDecorator = pieceMovePreview.GetComponent<SplineDecorator>();

            setColor(defaultColor);
        }

        internal void onMovePath(List<Tile> tiles, bool arcPath = false)
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
                var tilePosition = currentTile.fullPosition.AddY(height);

                moveSpline.SetControlPoint(anchorPointIndex, tilePosition);
                if (nextTile == null)
                {
                    if (!arcPath)
                    {
                        moveSpline.SetControlPoint(anchorPointIndex, tilePosition.AddY(-height * .75f));
                    }
                    break;
                }
                if (i > 0)
                {
                    moveSpline.AddCurve();
                }
                var diffVector = nextTile.fullPosition.AddY(height) - tilePosition;
                Vector3 secondControl, thirdControl;

                if (arcPath)
                {
                    secondControl = (diffVector * 0.2f) + (curveHeight * diffVector.magnitude) + tilePosition;
                    thirdControl = (diffVector * 0.8f) + (curveHeight * diffVector.magnitude) + tilePosition;
                }
                else
                {
                    secondControl = (diffVector * 0.5f) + tilePosition;
                    thirdControl = (diffVector * 0.5f) + tilePosition;
                }

                moveSpline.SetControlPoint(anchorPointIndex + 1, secondControl);
                moveSpline.SetControlPoint(anchorPointIndex + 2, thirdControl);
                //moveSpline.SetControlPoint(i + 3, nextTile.fullPosition);
            }

            pieceMovePreview.SetActive(true);
            if (arcPath)
            {
                splineDecorator.SetFrequency(6);
            }
            else
            {
                splineDecorator.SetFrequency(3 * (tiles.Count - 1));
            }
        }

        private Color lastColor; 
        internal void setColor(Color c)
        {
            if(c == lastColor) return;

            int childCount = splineDecorator.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = splineDecorator.transform.GetChild(i).gameObject;
                var renderer = child.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = c;
                }
            }

            lastColor = c;
        }

    }
}


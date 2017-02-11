using ctac.util;
using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public class TauntLinesView : View
    {
        GameObject tauntLoops;
        GameObject tauntLoopPrefab;

        private Color enemyColor = ColorExtensions.HexToColor("#E52600");
        private Color friendlyColor = ColorExtensions.HexToColor("#0057E5");

        [Inject]
        public IResourceLoaderService loader { get; set; }

        internal void init()
        {
            tauntLoops = GameObject.Find("TauntLoops");
            tauntLoopPrefab = loader.Load<GameObject>("TauntLoop");
        }

        void Update()
        {
        }

        //subtract out the line width
        const float tileHwidth = 0.5f;
        const float lineWidth = 0.05f;

        internal void ResetPerims()
        {
            tauntLoops.transform.DestroyChildren(true);
        }

        internal void UpdatePerims(List<List<Tile>> perims, MapModel map, bool isFriendly)
        {
            if(perims == null) return;

            foreach (var perim in perims)
            {
                var newLoop = GameObject.Instantiate(
                    tauntLoopPrefab, 
                    Vector3.zero,
                    Quaternion.identity
                ) as GameObject;
                newLoop.transform.parent = tauntLoops.transform;
                newLoop.name = "Loop ";
                var lineRenderer = newLoop.GetComponent<LineRenderer>();

                lineRenderer.endColor = isFriendly ? friendlyColor : enemyColor;
                lineRenderer.startColor = isFriendly ? friendlyColor : enemyColor;

                //go around the perimeter adding the line renderer points following the curvature of the EARF
                //assuming we're starting at the right most, top most point
                List<Vector3> perimPoints = new List<Vector3>();
                TTDir lastDirection = TTDir.Right;
                TTDir nextDirection;
                Tile firstTile = perim[0];
                Tile lastTile = null;
                Tile nextTile = null;
                for(int t = 0; t < perim.Count; t++) 
                {
                    var tile = perim[t];
                    if (lastTile != null)
                    {
                        lastDirection = tileDir(tile.position, lastTile.position);
                    }
                    if (t + 1 < perim.Count)
                    {
                        nextTile = perim[t + 1];
                    }
                    else
                    {
                        nextTile = firstTile;
                    }
                    nextDirection = tileDir(nextTile.position, tile.position);
                    var fullPos = tile.fullPosition;

                    //handle all the corner cases, hah, get it?
                    if (lastDirection == TTDir.Right && nextDirection == TTDir.Down)
                    {
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth, fullPos.y, fullPos.z + tileHwidth - lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth - lineWidth, fullPos.y, fullPos.z + tileHwidth - lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth - lineWidth, fullPos.y, fullPos.z - tileHwidth));
                    }
                    if (lastDirection == TTDir.Down && nextDirection == TTDir.Left)
                    {
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth - lineWidth, fullPos.y, fullPos.z + tileHwidth));
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth - lineWidth, fullPos.y, fullPos.z - tileHwidth + lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth, fullPos.y, fullPos.z - tileHwidth + lineWidth));
                    }
                    if (lastDirection == TTDir.Left && nextDirection == TTDir.Up)
                    {
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth, fullPos.y, fullPos.z - tileHwidth + lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth + lineWidth, fullPos.y, fullPos.z - tileHwidth + lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth + lineWidth, fullPos.y, fullPos.z + tileHwidth));
                    }
                    if (lastDirection == TTDir.Up && nextDirection == TTDir.Right)
                    {
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth + lineWidth, fullPos.y, fullPos.z - tileHwidth));
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth + lineWidth, fullPos.y, fullPos.z + tileHwidth - lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth, fullPos.y, fullPos.z + tileHwidth - lineWidth));
                    }

                    //now the straight line cases
                    if (lastDirection == TTDir.Up && nextDirection == TTDir.Up)
                    {
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth + lineWidth, fullPos.y, fullPos.z - tileHwidth));
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth + lineWidth, fullPos.y, fullPos.z + tileHwidth));
                    }
                    if (lastDirection == TTDir.Down && nextDirection == TTDir.Down)
                    {
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth - lineWidth, fullPos.y, fullPos.z + tileHwidth));
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth - lineWidth, fullPos.y, fullPos.z - tileHwidth));
                    }
                    if (lastDirection == TTDir.Left && nextDirection == TTDir.Left)
                    {
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth, fullPos.y, fullPos.z - tileHwidth + lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth, fullPos.y, fullPos.z - tileHwidth + lineWidth));
                    }
                    if (lastDirection == TTDir.Right && nextDirection == TTDir.Right)
                    {
                        perimPoints.Add(new Vector3(fullPos.x - tileHwidth, fullPos.y, fullPos.z + tileHwidth - lineWidth));
                        perimPoints.Add(new Vector3(fullPos.x + tileHwidth, fullPos.y, fullPos.z + tileHwidth - lineWidth));
                    }

                    //interior angles not needing any handling are:
                    //right && up
                    //down && right
                    //up && left
                    //left && down


                    lastTile = tile;
                }

                float totalLength = 0f;
                for (int p = 1; p < perimPoints.Count; p++)
                {
                    totalLength += Vector3.Distance(perimPoints[p], perimPoints[p - 1]);
                }

                lineRenderer.material.SetFloat("_RepeatCount", totalLength * 2.5f);

                lineRenderer.numPositions = perimPoints.Count;
                lineRenderer.SetPositions(perimPoints.ToArray());
            }

        }

        private TTDir tileDir(Vector2 currentTile, Vector2 lastTile)
        {
            var xDelta = currentTile.x - lastTile.x;
            var yDelta = currentTile.y - lastTile.y;

            if(xDelta > 0) return TTDir.Right;
            if(xDelta < 0) return TTDir.Left;
            if(yDelta > 0) return TTDir.Up;
            if(yDelta < 0) return TTDir.Down;

            return TTDir.Right;
        }

    }
}


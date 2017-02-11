using strange.extensions.mediation.impl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class TauntLinesView : View
    {
        GameObject tauntLoops;
        GameObject tauntLoopPrefab;
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

        internal void UpdatePerims(List<List<Tile>> perims, MapModel map)
        {
            tauntLoops.transform.DestroyChildren(true);
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

                lineRenderer.endColor = Color.red;
                lineRenderer.startColor = Color.red;

                var perimPoints = perim.Select(t => t.fullPosition).ToArray();
                lineRenderer.numPositions = perimPoints.Length;
                lineRenderer.SetPositions(perimPoints);
            }

        }

    }
}


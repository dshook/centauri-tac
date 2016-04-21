using strange.extensions.mediation.impl;
using System;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class PointerView : View
    {
        private GameObject pointerCurve;
        private BezierSpline pointerSpline;

        private Vector3 startPoint;
        private Vector3 curveHeight = new Vector3(0, 1.35f, 0);
        private RectTransform CanvasRect;
        private CardCanvasHelperView cardCanvasHelper;
        private int tileMask = 0;

        private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        internal void init()
        {
            pointerCurve = transform.Find("PointerCurve").gameObject;
            pointerCurve.SetActive(false);

            pointerSpline = pointerCurve.GetComponent<BezierSpline>();

            tileMask = LayerMask.GetMask("Tile");
            CanvasRect = GameObject.Find(Constants.cardCanvas).GetComponent<RectTransform>();
            cardCanvasHelper = GameObject.Find(Constants.cardCanvas).GetComponent<CardCanvasHelperView>();
        }

        void Update()
        {
            if (pointerCurve.activeSelf)
            {
                var ray = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Constants.cameraRaycastDist, tileMask))
                {
                    Vector3 mouseWorld = hit.point;

                    var diffVector = mouseWorld - startPoint;

                    var secondControl = (diffVector * 0.2f) + curveHeight + startPoint;
                    var thirdControl = (diffVector * 0.8f) + curveHeight + startPoint;

                    pointerSpline.SetControlPoint(0, startPoint);
                    pointerSpline.SetControlPoint(1, secondControl);
                    pointerSpline.SetControlPoint(2, thirdControl);
                    pointerSpline.SetControlPoint(3, mouseWorld);
                }
            }
        }

        internal void screenPointAt(Vector2 start)
        {
            startPoint = start;
            pointerCurve.SetActive(true);
        }

        internal void rectTransform(GameObject go)
        {
            var screenPoint = cardCanvasHelper.WorldToViewportPoint(go.transform.position);
            screenPoint.Scale(new Vector3(Camera.main.pixelWidth, 1, Camera.main.pixelHeight));

            var ray = Camera.main.ScreenPointToRay(screenPoint);
            float dist;
            if (groundPlane.Raycast(ray, out dist))
            {
                var worldPos = ray.GetPoint(dist);
                startPoint = worldPos.SetY(1f);
                pointerCurve.SetActive(true);
            }
        }

        internal void worldPoint(Transform t)
        {
            startPoint = t.position;
            pointerCurve.SetActive(true);
        }

        internal void disable()
        {
            pointerCurve.SetActive(false);
        }
    }
}


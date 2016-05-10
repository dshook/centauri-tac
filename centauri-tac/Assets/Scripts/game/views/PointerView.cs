using strange.extensions.mediation.impl;
using System;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class PointerView : View
    {
        GameObject pointerCurve;
        BezierSpline pointerSpline;
        RaycastModel raycastModel;

        Vector3 startPoint;
        Vector3 curveHeight = new Vector3(0, 1.35f, 0);
        CardCanvasHelperView cardCanvasHelper;

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        internal void init(RaycastModel rm)
        {
            raycastModel = rm;
            pointerCurve = transform.Find("PointerCurve").gameObject;
            pointerCurve.SetActive(false);

            pointerSpline = pointerCurve.GetComponent<BezierSpline>();

            cardCanvasHelper = GameObject.Find(Constants.cardCanvas).GetComponent<CardCanvasHelperView>();
        }

        void Update()
        {
            if (pointerCurve.activeSelf)
            {
                if (raycastModel.worldHit.HasValue)
                {
                    Vector3 mouseWorld = raycastModel.worldHit.Value.point;

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


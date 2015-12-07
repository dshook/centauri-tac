using strange.extensions.mediation.impl;
using System;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class PointerView : View
    {
        private MeshRenderer meshRenderer;
        private Vector2 startPoint;
        private RectTransform CanvasRect;
        private CardCanvasHelperView cardCanvasHelper;

        private Vector3 yForward = new Vector3(0, 1, 0);

        internal void init()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            CanvasRect = GameObject.Find(Constants.cardCanvas).GetComponent<RectTransform>();
            cardCanvasHelper = GameObject.Find(Constants.cardCanvas).GetComponent<CardCanvasHelperView>();
        }

        void Update()
        {
            if (meshRenderer.enabled)
            {
                Vector2 mouseScreen = CrossPlatformInputManager.mousePosition;

                //position
                var midViewport = ((mouseScreen- startPoint) * 0.5f) + startPoint;
                var midWorld = midViewport - (CanvasRect.sizeDelta / 2);
                transform.parent.localPosition = midWorld;

                //find angles
                //v1 is a vector pointing right because this is the default rotation of the arrow
                var v1 = Vector2.right;
                var v2 = mouseScreen - startPoint;
                var angle = Vector2.Angle(v1, v2);
                Vector3 cross = Vector3.Cross(mouseScreen, startPoint);
                if (cross.z > 0) angle -= 360;

                var newRotation = Quaternion.AngleAxis(angle, yForward);
                transform.localRotation = newRotation;

                //scale
                var distance = Vector2.Distance(startPoint, mouseScreen);
                transform.localScale = transform.localScale.SetZ(distance / 10);

                //Debug.Log(string.Format("Start {0} Mouse {1} Angle {2} Mid View {3} Mid World {4}", startPoint, mouseScreen, angle, midViewport, midWorld));
            }
        }

        internal void screenPointAt(Vector2 start)
        {
            meshRenderer.enabled = true;
            startPoint = start;
        }

        internal void rectTransform(GameObject go)
        {
            meshRenderer.enabled = true;

            var screenPos = cardCanvasHelper.WorldToViewport(go.transform.position);

            startPoint = new Vector2(Math.Max(screenPos.x, 0), Math.Max(screenPos.y, 0));
        }

        internal void disable()
        {
            meshRenderer.enabled = false;
        }
    }
}


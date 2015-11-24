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
        private Camera cardCamera;
        private RectTransform CanvasRect;

        private Vector3 yForward = new Vector3(0, 1, 0);

        internal void init()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == "CardCamera");
            meshRenderer.enabled = false;
            CanvasRect = GameObject.Find("cardCanvas").GetComponent<RectTransform>();
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

        internal void worldPointAt(Vector2 start)
        {
            meshRenderer.enabled = true;
            startPoint = cardCamera.WorldToScreenPoint(start);
        }

        internal void rectTransform(GameObject go)
        {
            meshRenderer.enabled = true;
            Vector2 ViewportPosition = cardCamera.WorldToViewportPoint(go.transform.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
                ((ViewportPosition.x * CanvasRect.sizeDelta.x)),
                ((ViewportPosition.y * CanvasRect.sizeDelta.y))
            );

            startPoint = new Vector2(Math.Max(WorldObject_ScreenPosition.x, 0), Math.Max(WorldObject_ScreenPosition.y, 0));
        }

        internal void disable()
        {
            meshRenderer.enabled = false;
        }
    }
}


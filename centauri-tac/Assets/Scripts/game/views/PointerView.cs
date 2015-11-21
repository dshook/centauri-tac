using strange.extensions.mediation.impl;
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
                Debug.Log("Mouse " + mouseScreen);

                //var rotateVector = Vector2.Dot(startPoint, mouseScreen);
                //var angle = Vector2.Angle(startPoint, mouseScreen);
                var distance = Vector2.Distance(startPoint, mouseScreen);
                //var up = mouseScreen - startPoint;
                //var newRotation = Quaternion.LookRotation(transform.forward, up);
                //transform.localRotation = newRotation;
                transform.localScale = transform.localScale.SetZ(distance / 8);
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

            startPoint = WorldObject_ScreenPosition;
        }

        internal void disable()
        {
            meshRenderer.enabled = false;
        }
    }
}


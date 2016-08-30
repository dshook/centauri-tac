using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac {
    public class CardCanvasHelperView : MonoBehaviour
    {
        private Camera cardCamera;
        private RectTransform CanvasRect;

        void Start()
        {
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == Constants.cardCamera);
            CanvasRect = GameObject.Find(Constants.cardCanvas).GetComponent<RectTransform>();
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            Vector2 ViewportPosition = cardCamera.WorldToViewportPoint(position);
            return new Vector3(
                ((ViewportPosition.x * CanvasRect.sizeDelta.x)),
                ((ViewportPosition.y * CanvasRect.sizeDelta.y)),
                position.z
            );
        }

        public Vector3 WorldToViewportPoint(Vector3 position)
        {
            return cardCamera.WorldToViewportPoint(position);
        }

        public Vector3 ViewportToWorld(Vector3 viewportPoint)
        {
            return cardCamera.ViewportToWorldPoint(viewportPoint);
        }

        public Vector3 MouseToWorld(float z)
        {
            var mouseViewport = cardCamera.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);

            //calculate the position of the UI element
            //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
            return new Vector3(
                ((mouseViewport.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
                ((mouseViewport.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)),
                z
            );
        }
    }
}

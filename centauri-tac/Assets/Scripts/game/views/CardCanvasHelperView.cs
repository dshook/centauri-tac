using System;
using System.Linq;
using UnityEngine;

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

        public Vector3 WorldToViewport(Vector3 position)
        {
            Vector2 ViewportPosition = cardCamera.WorldToViewportPoint(position);
            return new Vector3(
                ((ViewportPosition.x * CanvasRect.sizeDelta.x)),
                ((ViewportPosition.y * CanvasRect.sizeDelta.y)),
                position.z
            );
        }

    }
}

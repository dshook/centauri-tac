using UnityEngine;

namespace ctac
{
    public static class RectTransformExtensions
    {
        public static void SetAnchor(this RectTransform rectTransform, Vector2 anchor)
        {
            rectTransform.anchorMax = anchor;
            rectTransform.anchorMin = anchor;
            rectTransform.pivot = anchor;
        }
    }
}


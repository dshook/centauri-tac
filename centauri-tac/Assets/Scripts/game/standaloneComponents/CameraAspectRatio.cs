using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraAspectRatio : MonoBehaviour
{
    public float maxAspectNumerator = 16f;
    public float maxAspectDenomenator = 10f;

    public float minAspectNumerator = 4f;
    public float minAspectDenomenator = 3f;

    new Camera camera = null;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (camera == null)
        {
            camera = GetComponent<Camera>();
        }

        float targetAspectMax = maxAspectNumerator / maxAspectDenomenator;
        float targetAspectMin = minAspectNumerator / minAspectDenomenator;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        Rect rect = camera.rect;

        if (windowaspect > targetAspectMax)
        {
            // add pillarbox
            // current viewport height should be scaled by this amount
            float scalewidth = targetAspectMax / windowaspect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
        else if (windowaspect < targetAspectMin)
        {
            // if scaled height is less than current height, add letterbox
            float scaleheight = windowaspect / targetAspectMin;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            camera.rect = rect;
        }
        else
        {
            rect.width = 1.0f;
            rect.height = 1.0f;
            rect.x = 0;
            rect.y = 0;

            camera.rect = rect;
        }

    }
}

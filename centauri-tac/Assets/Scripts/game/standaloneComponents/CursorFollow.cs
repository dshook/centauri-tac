using System.Linq;
using ctac;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class CursorFollow : MonoBehaviour
{
    public Vector3 offset = Vector3.zero;

    RectTransform rectTransform;
    // CanvasScaler scaler ;
    // Camera cam;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // cam = Camera.allCameras.FirstOrDefault(x => x.name == Constants.cardCamera);
    }

    void Update()
    {

      //TODO: Somehow this is fucked up with different screen sizes where it doesn't stay in place

      if(rectTransform != null){
        rectTransform.anchoredPosition = CrossPlatformInputManager.mousePosition + offset;
      }
    }
}

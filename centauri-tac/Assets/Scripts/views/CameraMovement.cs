using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class CameraMovement : MonoBehaviour
{
    public float dragSpeed = 2;
    private Vector3 dragOrigin;
    bool dragging = false;

    void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            dragOrigin = Input.mousePosition;
            dragging = true;
            return;
        }

        if (CrossPlatformInputManager.GetButtonUp("Fire1"))
        {
            dragging = false;
            return;
        }

        if (dragging)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(pos.x * -dragSpeed, 0, pos.y * -dragSpeed);

            transform.Translate(move, Space.World);
        }
    }


}

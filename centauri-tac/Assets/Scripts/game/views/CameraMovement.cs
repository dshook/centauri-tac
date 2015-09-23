using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class CameraMovement : MonoBehaviour
{
    private Vector3 dragOrigin;
    private Vector3 camOrigin;

    private Vector3 mouseDiff;
    private Vector3 amountToMove;
    private Vector3 move;

    bool dragging = false;
    float xSpeed = 10f;
    float ySpeed = 5f;

    void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            dragOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            camOrigin = transform.position;
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
            var mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            mouseDiff = mousePos - dragOrigin;

            //TODO: speed is still not right
            amountToMove.Set(-mouseDiff.x * xSpeed, -mouseDiff.y * ySpeed, 0);
            amountToMove = Camera.main.transform.rotation * amountToMove;
            move = camOrigin + amountToMove;

            transform.position = Vector3.MoveTowards(transform.position, move, 1f);
        }
    }
}

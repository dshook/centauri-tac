using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using ctac;

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
            //test click position to see if we hit the ground
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                if (objectHit.collider.gameObject.CompareTag("Tile"))
                {
                    dragOrigin = Camera.main.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
                    camOrigin = transform.position;
                    dragging = true;
                    return;
                }
            }
        }

        if (CrossPlatformInputManager.GetButtonUp("Fire1"))
        {
            dragging = false;
            return;
        }

        if (dragging)
        {
            var mousePos = Camera.main.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
            mouseDiff = mousePos - dragOrigin;

            //TODO: speed is still not right
            amountToMove.Set(-mouseDiff.x * xSpeed, -mouseDiff.y * ySpeed, 0);
            amountToMove = Camera.main.transform.rotation * amountToMove;
            move = camOrigin + amountToMove;

            transform.position = Vector3.MoveTowards(transform.position, move, 1f);
        }
    }
}

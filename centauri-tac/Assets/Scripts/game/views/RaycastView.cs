using strange.extensions.mediation.impl;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class RaycastView : View
    {
        [Inject] public RaycastModel model { get; set; }
        [Inject] public MapModel map { get; set; } 
        private Camera cardCamera;

        int cardCanvasLayer = -1;
        int tileMask = -1;
        new void Awake()
        {
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == Constants.cardCamera);
            cardCanvasLayer = LayerMask.GetMask(Constants.cardCanvas);
            tileMask = LayerMask.GetMask("Tile");
        }

        void Update()
        {

            //camera movement
            //test click position to see if we hit the ground
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit tileHit;
            if (Physics.Raycast(camRay, out tileHit, Constants.cameraRaycastDist, tileMask))
            {
                model.tile = map.tiles.Get(tileHit.collider.gameObject.transform.position.ToTileCoordinates());
            }
            else
            {
                model.tile = null;
            }

            RaycastHit objectHit;
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                if (objectHit.collider.gameObject.CompareTag("Piece"))
                {
                    model.piece = objectHit.collider.gameObject.GetComponent<PieceView>();
                }
                else
                {
                    model.piece = null;
                }
                model.worldHit = objectHit;
            }
            else
            {
                model.worldHit = null;
            }

            //card interaction selection
            var viewportPoint = cardCamera.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
            camRay = cardCamera.ViewportPointToRay(viewportPoint);

            RaycastHit cardHit;
            if (Physics.Raycast(camRay, out cardHit, Constants.cameraRaycastDist, cardCanvasLayer))
            {
                model.cardCanvasHit = cardHit;
            }
            else
            {
                model.cardCanvasHit = null;
            }

        }
    }
}


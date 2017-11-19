using strange.extensions.mediation.impl;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class RaycastView : View
    {
        [Inject] public RaycastModel model { get; set; }
        [Inject] public MapModel map { get; set; } 
        private Camera cardCamera;
        EventSystem eventSystem;

        int cardCanvasLayer = -1;
        int tileLayer = -1;
        new void Awake()
        {
            cardCamera = Camera.allCameras.FirstOrDefault(x => x.name == Constants.cardCamera);
            cardCanvasLayer = LayerMask.GetMask(Constants.cardCanvas);
            tileLayer = LayerMask.GetMask("Tile");
            eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        }

        void Update()
        {
            //Reset everything
            model.tile = null;
            model.piece = null;
            model.worldHit = null;
            model.cardCanvasHit = null;

            //Ignore any raycasts if we're over a UI object
            if (eventSystem.IsPointerOverGameObject())
            {
                return;
            }

            //test click position to see if we hit the ground
            Ray camRay = Camera.main.ScreenPointToRay(CrossPlatformInputManager.mousePosition);

            RaycastHit tileHit;
            if (Physics.Raycast(camRay, out tileHit, Constants.cameraRaycastDist, tileLayer))
            {
                model.tile = map.tiles.Get(tileHit.collider.gameObject.transform.position.ToTileCoordinates());
            }

            RaycastHit objectHit;
            //world hit for everything else, but also ignore UI layer hits
            if (Physics.Raycast(camRay, out objectHit, Constants.cameraRaycastDist))
            {
                //walk up 3 levels at most to try to see if we're in a piece
                //kinda nasty but avoids having to remember to set the piece tag on every single piece
                var hitGO = objectHit.collider.gameObject;
                if (hitGO.CompareTag("Piece")
                    || (hitGO.transform.parent != null && hitGO.transform.parent.CompareTag("Piece"))
                    || (hitGO.transform.parent != null && hitGO.transform.parent.parent != null && hitGO.transform.parent.parent.CompareTag("Piece"))
                    || (hitGO.transform.parent != null && hitGO.transform.parent.parent != null && hitGO.transform.parent.parent.parent != null && hitGO.transform.parent.parent.parent.CompareTag("Piece"))
                )
                {
                    model.piece = objectHit.collider.gameObject.GetComponentInParent<PieceView>();
                }
                model.worldHit = objectHit;
            }

            //card interaction selection
            var viewportPoint = cardCamera.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
            camRay = cardCamera.ViewportPointToRay(viewportPoint);

            RaycastHit cardHit;
            if (Physics.Raycast(camRay, out cardHit, Constants.cameraRaycastDist, cardCanvasLayer))
            {
                model.cardCanvasHit = cardHit;
            }


        }
    }
}


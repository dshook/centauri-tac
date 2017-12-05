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
        public bool enableUICasts;
        private Camera cardCamera;
        EventSystem eventSystem;

        int cardCanvasLayer = -1;
        int tileLayer = -1;
        new void Awake()
        {
            base.Awake();
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
            if (eventSystem.IsPointerOverGameObject() && !enableUICasts)
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
                var hitGO = objectHit.collider.gameObject;
                if (hitGO.CompareTag("Piece"))
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


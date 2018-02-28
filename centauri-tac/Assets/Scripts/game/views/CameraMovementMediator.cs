using strange.extensions.mediation.impl;
using ctac.signals;
using UnityEngine;
using System.Linq;

namespace ctac
{
    public class CameraMovementMediator : Mediator
    {
        [Inject] public CameraMovementView view { get; set; }

        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public MapModel map { get; set; }

        public override void OnRegister()
        {
            view.Init(raycastModel);
        }

        [ListensTo(typeof(HistoryHoverSignal))]
        public void onHistoryHover(bool h)
        {
            view.zoomEnabled = !h;
        }

        [ListensTo(typeof(MoveCameraToTileSignal))]
        public void onMoveCamera(Vector2 pos)
        {
            view.MoveToTile(pos);
        }

        [ListensTo(typeof(PieceSpawnedSignal))]
        public void onPieceSpawned(PieceSpawnedModel piece)
        {
            if (piece.piece.isHero && piece.piece.playerId == players.Me.id) {
                view.MoveToTile(piece.piece.tilePosition);
            }
        }

        [ListensTo(typeof(MapCreatedSignal))]
        public void onMapCreated()
        {
            //calculate and update camera movement bounds based on map size
            var minX = map.tileList.Min(t => t.position.x);
            var maxX = map.tileList.Max(t => t.position.x);
            var minZ = map.tileList.Min(t => t.position.y);
            var maxZ = map.tileList.Max(t => t.position.y);

            float camMargin = 16f;

            view.camBounds = new Vector4(
                minX - camMargin,
                minZ - camMargin,
                maxX + camMargin,
                maxZ + camMargin);
        }
    }
}


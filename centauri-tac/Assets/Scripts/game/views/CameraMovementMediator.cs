using strange.extensions.mediation.impl;
using ctac.signals;
using UnityEngine;

namespace ctac
{
    public class CameraMovementMediator : Mediator
    {
        [Inject] public CameraMovementView view { get; set; }

        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

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
    }
}


using strange.extensions.mediation.impl;
using ctac.signals;
using UnityEngine;

namespace ctac
{
    public class CameraMovementMediator : Mediator
    {
        [Inject] public CameraMovementView view { get; set; }

        [Inject] public CardSelectedSignal cardSelected { get; set; }

        [Inject] public StartSelectTargetSignal startTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelTarget { get; set; }
        [Inject] public SelectTargetSignal targetSelected { get; set; }
        [Inject] public PieceSpawnedSignal pieceSpawned { get; set; }

        [Inject] public HistoryHoverSignal historyHover { get; set; }
        [Inject] public MoveCameraToTileSignal moveCamSignal { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        public override void OnRegister()
        {
            cardSelected.AddListener(onCardSelected);
            startTarget.AddListener(onStartTarget);
            cancelTarget.AddListener(onCancelTarget);
            targetSelected.AddListener(onSelectTarget);
            historyHover.AddListener(onHistoryHover);
            pieceSpawned.AddListener(onPieceSpawned);
            moveCamSignal.AddListener(onMoveCamera);

            view.Init(raycastModel);
        }

        public override void OnRemove()
        {
            cardSelected.RemoveListener(onCardSelected);
            startTarget.RemoveListener(onStartTarget);
            cancelTarget.RemoveListener(onCancelTarget);
            targetSelected.RemoveListener(onSelectTarget);
            pieceSpawned.RemoveListener(onPieceSpawned);
            moveCamSignal.RemoveListener(onMoveCamera);
        }

        private void onCardSelected(CardSelectedModel card)
        {
            view.setDragEnabled(card == null);
        }

        private void onStartTarget(TargetModel m)
        {
            //view.setDragEnabled(false);
        }
        private void onSelectTarget(TargetModel c)
        {
            //view.setDragEnabled(true);
        }
        private void onCancelTarget(CardModel c)
        {
            //view.setDragEnabled(true);
        }

        private void onHistoryHover(bool h)
        {
            view.zoomEnabled = !h;
        }

        private void onMoveCamera(Vector2 pos)
        {
            view.MoveToTile(pos);
        }

        public void onPieceSpawned(PieceSpawnedModel piece)
        {
            if (piece.piece.isHero && piece.piece.playerId == players.Me.id) {
                view.MoveToTile(piece.piece.tilePosition);
            }
        }
    }
}


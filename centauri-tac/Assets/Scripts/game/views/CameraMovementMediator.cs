using strange.extensions.mediation.impl;
using ctac.signals;
using UnityEngine;

namespace ctac
{
    public class CameraMovementMediator : Mediator
    {
        [Inject] public CameraMovementView view { get; set; }

        [Inject] public CardSelectedSignal cardSelected { get; set; }
        [Inject] public PieceSelectedSignal pieceSelected { get; set; }

        [Inject] public StartSelectTargetSignal startTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelTarget { get; set; }
        [Inject] public SelectTargetSignal targetSelected { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }

        public override void OnRegister()
        {
            cardSelected.AddListener(onCardSelected);
            startTarget.AddListener(onStartTarget);
            cancelTarget.AddListener(onCancelTarget);
            targetSelected.AddListener(onSelectTarget);
            pieceSelected.AddListener(onPieceSelected);

            view.Init(raycastModel);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
            startTarget.RemoveListener(onStartTarget);
            cancelTarget.RemoveListener(onCancelTarget);
            targetSelected.RemoveListener(onSelectTarget);
            pieceSelected.RemoveListener(onPieceSelected);
        }

        private void onCardSelected(CardModel card)
        {
            view.setDragEnabled(card != null);
        }

        private void onStartTarget(TargetModel m)
        {
            view.setDragEnabled(true);
        }
        private void onSelectTarget(TargetModel c)
        {
            view.setDragEnabled(false);
        }
        private void onCancelTarget(CardModel c)
        {
            view.setDragEnabled(false);
        }

        private void onPieceSelected(PieceModel p)
        {
            view.setDragEnabled(p == null);
        }
    }
}


using strange.extensions.mediation.impl;
using ctac.signals;
using UnityEngine;

namespace ctac
{
    public class CameraMovementMediator : Mediator
    {
        [Inject]
        public CameraMovementView view { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public StartSelectTargetSignal startTarget { get; set; }

        [Inject]
        public CancelSelectTargetSignal cancelTarget { get; set; }

        [Inject]
        public SelectTargetSignal targetSelected { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        public override void OnRegister()
        {
            cardSelected.AddListener(onCardSelected);
            startTarget.AddListener(onStartTarget);
            cancelTarget.AddListener(onCancelTarget);
            targetSelected.AddListener(onSelectTarget);
            view.CameraRotated.AddListener(onCameraRotate);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
            startTarget.RemoveListener(onStartTarget);
            cancelTarget.RemoveListener(onCancelTarget);
            targetSelected.RemoveListener(onSelectTarget);
            view.CameraRotated.RemoveListener(onCameraRotate);
        }

        private void onCardSelected(CardModel card)
        {
            view.onCardSelected(card != null);
        }

        private void onStartTarget(StartTargetModel m)
        {
            view.onCardSelected(true);
        }
        private void onSelectTarget(StartTargetModel c, PieceModel m)
        {
            view.onCardSelected(false);
        }
        private void onCancelTarget(CardModel c)
        {
            view.onCardSelected(false);
        }

        private void onCameraRotate(float newRotation)
        {
            //gotta rotate all the pieces round for now
            foreach (var piece in pieces.Pieces)
            {
                var curRot = piece.gameObject.transform.rotation.eulerAngles;
                var newQuaternion = Quaternion.Euler(curRot.x, newRotation - 45, curRot.z);
                piece.gameObject.transform.rotation = newQuaternion;
            }
        }
    }
}


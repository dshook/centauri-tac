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
        public PiecesModel pieces { get; set; }

        public override void OnRegister()
        {
            cardSelected.AddListener(onCardSelected);
            view.CameraRotated.AddListener(onCameraRotate);
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
            view.CameraRotated.RemoveListener(onCameraRotate);
        }

        private void onCardSelected(CardModel card)
        {
            view.onCardSelected(card != null);
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


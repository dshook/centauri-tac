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
        }

        public override void onRemove()
        {
            base.onRemove();
            cardSelected.RemoveListener(onCardSelected);
            startTarget.RemoveListener(onStartTarget);
            cancelTarget.RemoveListener(onCancelTarget);
            targetSelected.RemoveListener(onSelectTarget);
        }

        private void onCardSelected(CardModel card)
        {
            view.onCardSelected(card != null);
        }

        private void onStartTarget(StartTargetModel m)
        {
            view.onCardSelected(true);
        }
        private void onSelectTarget(StartTargetModel c, SelectTargetModel m)
        {
            view.onCardSelected(false);
        }
        private void onCancelTarget(CardModel c)
        {
            view.onCardSelected(false);
        }
    }
}


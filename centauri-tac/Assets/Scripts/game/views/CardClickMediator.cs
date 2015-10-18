using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class CardClickMediator : Mediator
    {
        [Inject]
        public CardClickView view { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.init();
        }

        public override void onRemove()
        {
        }

        private void onClick(GameObject clickedObject)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Card"))
                {
                    var cardView = clickedObject.GetComponent<CardView>();

                    cardSelected.Dispatch(cardView.card);
                }
            }
            else
            {
                cardSelected.Dispatch(null);
            }

        }
    }
}


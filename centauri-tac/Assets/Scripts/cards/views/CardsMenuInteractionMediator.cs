using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class CardsMenuInteractionMediator : Mediator
    {
        [Inject] public CardsMenuInteractionView view { get; set; }

        [Inject] public CardSelectedSignal cardSelected { get; set; }
        [Inject] public CardHoveredSignal cardHovered { get; set; }
        [Inject] public ActivateCardSignal activateCard { get; set; }
        [Inject] public MessageSignal message { get; set; }

        [Inject] public CardsModel cards { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }

        private CardModel draggedCard = null;

        [Inject] public IDebugService debug { get; set; }
        [Inject] public ISoundService sounds { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.activateSignal.AddListener(onActivate);
            view.hoverSignal.AddListener(onHover);
            activateCard.AddListener(onCardActivated);
            view.init(raycastModel);
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.activateSignal.RemoveListener(onActivate);
            view.hoverSignal.RemoveListener(onHover);
            activateCard.RemoveListener(onCardActivated);
        }

        //for clicking on a card directly
        private void onClick(GameObject clickedObject, Vector3 point)
        {
            if (clickedObject != null)
            {
                var cardView = clickedObject.GetComponent<CardView>();
                if (cardView == null) { return; }
                
                draggedCard = cardView.card;
            }
            else
            {
                draggedCard = null;
                cardSelected.Dispatch(null);
            }
        }

        //when you try to activate a card either by the click and drag click up or a click on a tile/piece
        private void onActivate(GameObject activated)
        {
            sounds.PlaySound("playCard");
            var itWorked = doActivateWork(activated);
            if (itWorked)
            {
                view.ClearDrag();
            }
        }

        //returns whether or not the activate was a good one or not
        private bool doActivateWork(GameObject activated)
        {
            if (activated == null || draggedCard == null)
            {
                cardSelected.Dispatch(null);
                return false;
            }
            return false;
        }

        private void onCardActivated(ActivateModel a)
        {
            cardSelected.Dispatch(null);
        }

        private CardView lastHoveredCard = null;
        private void onHover(GameObject hoveredObject)
        {
            if (hoveredObject != null)
            {
                if (hoveredObject.CompareTag("Card"))
                {
                    var cardView = hoveredObject.GetComponent<CardView>();
                    if (cardView != null && cardView != lastHoveredCard)
                    {
                        lastHoveredCard = cardView;
                        cardHovered.Dispatch(cardView.card);
                        cardView.EnableHoverTips(loader);
                    }
                }
            }
            else
            {
                if (lastHoveredCard != null)
                {
                    lastHoveredCard.DisableHoverTips();
                    lastHoveredCard = null;
                    cardHovered.Dispatch(null);
                }
            }
        }
    }
}


using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class CardInteractionMediator : Mediator
    {
        [Inject]
        public CardInteractionView view { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public CardHoveredSignal cardHovered { get; set; }

        [Inject]
        public ActivateCardSignal activateCard { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public MapModel map { get; set; }

        private CardModel draggedCard = null;
        private CardView lastHoveredCard = null;

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.activateSignal.AddListener(onActivate);
            view.hoverSignal.AddListener(onHover);
            view.init();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.activateSignal.RemoveListener(onActivate);
            view.hoverSignal.RemoveListener(onHover);
        }


        private void onClick(GameObject clickedObject)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Card"))
                {
                    var cardView = clickedObject.GetComponent<CardView>();

                    draggedCard = cardView.card;
                    cardSelected.Dispatch(draggedCard);
                }
            }
            else
            {
                draggedCard = null;
                cardSelected.Dispatch(null);
            }
        }

        private void onActivate(GameObject activated)
        {
            if (activated != null && draggedCard != null)
            {
                if (activated.CompareTag("Tile"))
                {
                    var gameTile = map.tiles.Get(activated.transform.position.ToTileCoordinates());

                    activateCard.Dispatch(draggedCard, gameTile);
                }
            }
        }

        private void onHover(GameObject hoveredObject)
        {
            if (hoveredObject != null)
            {
                if (hoveredObject.CompareTag("Card"))
                {
                    var cardView = hoveredObject.GetComponent<CardView>();
                    if (cardView != null && cardView != lastHoveredCard )
                    {
                        //break out and don't hover if it hasn't been added to the hand of cards yet
                        if (!cards.Cards.Contains(cardView.card))
                        {
                            return;
                        }
                        lastHoveredCard = cardView;
                        cardHovered.Dispatch(cardView.card);

                    }
                }
            }
            else
            {
                lastHoveredCard = null;
                cardHovered.Dispatch(null);
            }
        }
    }
}


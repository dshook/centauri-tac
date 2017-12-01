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
        [Inject] public MiniCardHoveredSignal miniCardHovered { get; set; }
        [Inject] public ActivateCardSignal activateCard { get; set; }
        [Inject] public MessageSignal message { get; set; }

        [Inject] public AddCardToDeckSignal addCardToDeck { get; set; }
        [Inject] public RemoveCardFromDeckSignal removeCardFromDeck { get; set; }
        [Inject] public SelectDeckSignal selectDeck { get; set; }

        [Inject] public CardsModel cards { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }

        private CardModel draggedCard = null;

        [Inject] public IDebugService debug { get; set; }
        [Inject] public ISoundService sounds { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onClick);
            view.hoverSignal.AddListener(onHover);

            view.init(raycastModel);
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onClick);
            view.hoverSignal.RemoveListener(onHover);
        }

        //for clicking on a thing.
        //TODO: prolly a better way to do this by tag or something
        private void onClick(GameObject clickedObject, Vector3 point)
        {
            if (clickedObject == null)
            {
                draggedCard = null;
                cardSelected.Dispatch(null);
                return;
            }

            var cardView = clickedObject.GetComponent<CardView>();
            if (cardView != null)
            {
                draggedCard = cardView.card;
                addCardToDeck.Dispatch(draggedCard);
                return;
            }

            var miniCardView = clickedObject.GetComponent<MiniCardView>();
            if (miniCardView != null)
            {
                removeCardFromDeck.Dispatch(miniCardView.card);
            }

            var deckList = clickedObject.GetComponent<DeckListView>();
            if (deckList != null)
            {
                selectDeck.Dispatch(deckList.deck);
            }
        }

        private CardView lastHoveredCard = null;
        private MiniCardView lastHoveredMiniCard = null;
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
                        //cardHovered.Dispatch(cardView.card);
                        cardView.EnableHoverTips(loader);
                    }
                }
                if (hoveredObject.CompareTag("MiniCard"))
                {
                    var cardView = hoveredObject.GetComponent<MiniCardView>();
                    if (cardView != null && cardView != lastHoveredMiniCard)
                    {
                        lastHoveredMiniCard = cardView;
                        miniCardHovered.Dispatch(cardView.card);
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
                if (lastHoveredMiniCard != null)
                {
                    lastHoveredMiniCard = null;
                    miniCardHovered.Dispatch(null);
                }
            }
        }
    }
}


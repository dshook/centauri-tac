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

        private string hoverName = "Hover Card";
        private CardView hoverCardView = null;

        private CardView lastHoveredCard = null;

        private Vector2 anchorPosition = new Vector2(0.5f, 0);

        public override void OnRegister()
        {
            initHoverCard();

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

        private void initHoverCard()
        {
            //init the hover card that's hidden most of the time
            var cardPrefab = Resources.Load("Card") as GameObject;
            var cardCanvas = GameObject.Find("cardCanvas");

            var hoverCardGO = GameObject.Instantiate(
                cardPrefab,
                new Vector3(10000,10000, 0),
                Quaternion.identity
            ) as GameObject;
            hoverCardGO.transform.SetParent(cardCanvas.transform, false);
            hoverCardGO.name = hoverName;

            var hoverCardModel = new CardModel()
            {
                playerId = -1,
                gameObject = hoverCardGO
            };

            hoverCardView = hoverCardGO.AddComponent<CardView>();
            hoverCardView.card = hoverCardModel;

            var rectTransform = hoverCardView.GetComponent<RectTransform>();
            rectTransform.anchorMax = anchorPosition;
            rectTransform.anchorMin = anchorPosition;
            rectTransform.pivot = anchorPosition;

            hoverCardGO.SetActive(false);
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

        private static Vector3 HoverOffset = new Vector3(0, 305, -1);
        private float hoverDelay = 0.5f;
        private void onHover(GameObject hoveredObject, float timeElapsed)
        {
            if (hoveredObject != null)
            {
                if (hoveredObject.CompareTag("Card"))
                {
                    var cardView = hoveredObject.GetComponent<CardView>();
                    if (cardView != null 
                        && cardView != lastHoveredCard 
                        && cardView != hoverCardView
                    )
                    {
                        //break out and don't hover if it hasn't been added to the hand of cards yet
                        if (!cards.Cards.Contains(cardView.card))
                        {
                            return;
                        }
                        cardHovered.Dispatch(cardView.card);

                        if (timeElapsed > hoverDelay)
                        {
                            lastHoveredCard = cardView;

                            //copy over props from hovered to hover
                            lastHoveredCard.card.CopyProperties(hoverCardView.card);
                            //but reset some key things
                            hoverCardView.name = hoverName;
                            hoverCardView.card.gameObject = hoverCardView.gameObject;

                            var rectTransform = hoverCardView.GetComponent<RectTransform>();
                            var hoveredCardRect = lastHoveredCard.GetComponent<RectTransform>();

                            rectTransform.anchoredPosition3D = hoveredCardRect.anchoredPosition3D + HoverOffset;

                            hoverCardView.gameObject.SetActive(true);
                        }
                    }
                }
            }
            else
            {
                hoverCardView.gameObject.SetActive(false);
                lastHoveredCard = null;
                cardHovered.Dispatch(null);
            }
        }
    }
}


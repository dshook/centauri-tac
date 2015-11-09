using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using strange.extensions.context.api;

namespace ctac
{
    public class CardHoverMediator : Mediator
    {
        [Inject]
        public CardHoverView view { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public ActivateCardSignal activateCard { get; set; }

        [Inject]
        public MapModel map { get; set; }

        private CardView hoverCard = null;

        private CardView hoveredCard = null;

        private Vector2 anchorPosition = new Vector2(0.5f, 0);

        public override void OnRegister()
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
            hoverCardGO.name = "Hover Card";

            var hoverCardModel = new CardModel()
            {
                playerId = -1,
                gameObject = hoverCardGO
            };

            hoverCard = hoverCardGO.AddComponent<CardView>();
            hoverCard.card = hoverCardModel;

            var rectTransform = hoverCard.GetComponent<RectTransform>();
            rectTransform.anchorMax = anchorPosition;
            rectTransform.anchorMin = anchorPosition;
            rectTransform.pivot = anchorPosition;

            hoverCardGO.SetActive(false);

            view.hoverSignal.AddListener(onHover);
            view.init();
        }

        public override void onRemove()
        {
            view.hoverSignal.RemoveListener(onHover);
        }

        private static Vector3 HoverOffset = new Vector3(0, 190, 0);
        private void onHover(GameObject hoveredObject)
        {
            if (hoveredObject != null)
            {
                if (hoveredObject.CompareTag("Card"))
                {
                    var cardView = hoveredObject.GetComponent<CardView>();
                    if (cardView != hoveredCard)
                    {
                        hoveredCard = cardView;
                        
                        //copy over props from hovered to hover
                        cardView.card.CopyProperties(hoverCard.card);
                        //but reset some key things
                        hoverCard.name = "Hover Card";
                        hoverCard.card.gameObject = hoverCard.gameObject;

                        var rectTransform = hoverCard.GetComponent<RectTransform>();
                        var hoveredCardRect = hoveredCard.GetComponent<RectTransform>();

                        rectTransform.anchoredPosition3D = hoveredCardRect.anchoredPosition3D + HoverOffset;

                        hoverCard.gameObject.SetActive(true);
                    }

                }
            }
            else
            {
                hoverCard.gameObject.SetActive(false);
                hoveredCard = null;
            }
        }
    }
}


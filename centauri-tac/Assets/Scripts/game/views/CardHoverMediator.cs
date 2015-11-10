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

        private string hoverName = "Hover Card";
        private CardView hoverCardView = null;

        private CardView lastHoveredCard = null;

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

            view.hoverSignal.AddListener(onHover);
            view.init();
        }

        public override void onRemove()
        {
            view.hoverSignal.RemoveListener(onHover);
        }

        private static Vector3 HoverOffset = new Vector3(0, 320, -1);
        private void onHover(GameObject hoveredObject)
        {
            if (hoveredObject != null)
            {
                if (hoveredObject.CompareTag("Card"))
                {
                    var cardView = hoveredObject.GetComponent<CardView>();
                    if (cardView != null && cardView != lastHoveredCard && cardView != hoverCardView)
                    {
                        Debug.Log("Hovered Diff Card " + cardView.name);
                        lastHoveredCard = cardView;

                        //copy over props from hovered to hover
                        cardView.card.CopyProperties(hoverCardView.card);
                        //but reset some key things
                        hoverCardView.name = hoverName;
                        hoverCardView.card.gameObject = hoverCardView.gameObject;

                        var rectTransform = hoverCardView.GetComponent<RectTransform>();
                        var hoveredCardRect = lastHoveredCard.GetComponent<RectTransform>();

                        rectTransform.anchoredPosition3D = hoveredCardRect.anchoredPosition3D + HoverOffset;

                        hoverCardView.gameObject.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("Hovered Same Card");
                    }

                }
            }
            else
            {
                Debug.Log("Hovered no Card");
                if (lastHoveredCard != null)
                {
                    Debug.Log("Hovered no Card state change");
                }
                hoverCardView.gameObject.SetActive(false);
                lastHoveredCard = null;
            }
        }
    }
}


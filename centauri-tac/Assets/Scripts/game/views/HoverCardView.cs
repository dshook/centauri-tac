using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class HoverCardView : View
    {
        internal Signal<GameObject> pieceHover = new Signal<GameObject>();

        float timer = 0f;

        private string hoverName = "Hover Card";
        private CardView hoverCardView = null;

        private Vector2 anchorPosition = new Vector2(0.5f, 0);

        internal void init()
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

        void Update()
        {
            timer += Time.deltaTime;
        }

        private static Vector3 HoverOffset = new Vector3(0, 305, -1);
        //private float hoverDelay = 0.5f;
        internal void showCard(CardModel cardToShow, Vector3 position)
        {
            Debug.Log("show hover");
            //copy over props from hovered to hover
            cardToShow.CopyProperties(hoverCardView.card);
            //but reset some key things
            hoverCardView.name = hoverName;
            hoverCardView.card.gameObject = hoverCardView.gameObject;

            var rectTransform = hoverCardView.GetComponent<RectTransform>();

            rectTransform.anchoredPosition3D = position  + HoverOffset;

            hoverCardView.gameObject.SetActive(true);
        }

        internal void hideCard()
        {
            Debug.Log("hide hover");
            hoverCardView.gameObject.SetActive(false);
        }
    }
}


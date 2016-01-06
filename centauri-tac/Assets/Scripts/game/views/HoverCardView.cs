using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using ctac;
using UnityStandardAssets.CrossPlatformInput;
using System.Linq;

namespace ctac
{
    public class HoverCardView : View
    {
        internal Signal<GameObject> pieceHover = new Signal<GameObject>();

        float timer = 0f;

        private string hoverName = "Hover Card";
        private CardView hoverCardView = null;

        private Vector2 cardAnchor = new Vector2(0.5f, 0);
        //private Vector2 centerAnchor = new Vector2(0.5f, 0.5f);
        private Vector2 bottomLeftAnchor = new Vector2(0, 0);
        private RectTransform rectTransform;

        internal void init()
        {
            //init the hover card that's hidden most of the time
            var cardPrefab = Resources.Load("Card") as GameObject;
            var cardCanvas = GameObject.Find(Constants.cardCanvas);

            var hoverCardGO = GameObject.Instantiate(
                cardPrefab,
                new Vector3(10000,10000, 0),
                Quaternion.identity
            ) as GameObject;
            hoverCardGO.transform.SetParent(cardCanvas.transform, false);
            hoverCardGO.name = hoverName;
            hoverCardGO.tag = "HoverCard";

            var hoverCardModel = new CardModel()
            {
                playerId = -1,
                gameObject = hoverCardGO
            };

            hoverCardView = hoverCardGO.AddComponent<CardView>();
            hoverCardView.card = hoverCardModel;

            rectTransform = hoverCardView.GetComponent<RectTransform>();


            hoverCardGO.SetActive(false);
        }

        void Update()
        {
            timer += Time.deltaTime;
        }

        //private float hoverDelay = 0.5f;
        internal void showCard(CardModel cardToShow, Vector3 position)
        {
            //copy over props from hovered to hover
            cardToShow.CopyProperties(hoverCardView.card);
            //but reset some key things
            hoverCardView.name = hoverName;
            hoverCardView.card.gameObject = hoverCardView.gameObject;

            rectTransform = hoverCardView.GetComponent<RectTransform>();

            rectTransform.anchoredPosition3D = position;

            hoverCardView.gameObject.SetActive(true);
        }

        private static Vector3 HoverOffset = new Vector3(0, 305, -1);
        internal void showCardFromHand(CardModel cardToShow, Vector3 position)
        {
            rectTransform.SetAnchor(cardAnchor);
            showCard(cardToShow, position + HoverOffset);
        }

        internal void showCardWorld(CardModel cardToShow, Vector3 worldPosition)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

            var hWidth = rectTransform.sizeDelta;
            rectTransform.SetAnchor(bottomLeftAnchor);
            //screenPos = screenPos.SetZ(0);

            var offsets = new[] {
                new Vector2(-hWidth.x, 0),
                new Vector2(hWidth.x, 0),
                new Vector2(0, -hWidth.y * .75f),
                new Vector2(0, hWidth.y * .75f)
            };

            foreach (var offset in offsets)
            {
                if (onScreen(screenPos + offset, hWidth))
                {
                    showCard(cardToShow, screenPos + offset);
                    break;
                }
            }
        }

        internal bool onScreen(Vector2 position, Vector2 hWidth)
        {
            var r1 = Camera.main.pixelRect;
            var r2 = new Rect(position - (hWidth / 2), hWidth);
            return r1.xMin < r2.xMin 
                && r1.xMax > r2.xMax 
                && r1.yMin < r2.yMin 
                && r1.yMax > r2.yMax;
        }

        internal void hideCard()
        {
            hoverCardView.gameObject.SetActive(false);
        }
    }
}


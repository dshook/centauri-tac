using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ctac
{
    public class HoverCardView : View
    {
        internal Signal<GameObject> pieceHover = new Signal<GameObject>();
        [Inject]
        public IResourceLoaderService loader { get; set; }
        [Inject]
        public IDebugService debug { get; set; }
        [Inject]
        public IPieceService pieceService { get; set; }

        float timer = 0f;
        bool cardVisible = false;
        bool active = true;

        private string hoverName = "Hover Card";
        private CardView hoverCardView = null;

        private Vector2 cardAnchor = new Vector2(0.5f, 0);
        //private Vector2 centerAnchor = new Vector2(0.5f, 0.5f);
        private Vector2 bottomLeftAnchor = new Vector2(0, 0);
        private CardDirectory cardDirectory;

        internal void init()
        {
            //init the hover card that's hidden most of the time
            var cardPrefab = loader.Load<GameObject>("Card");
            var cardCanvas = GameObject.Find(Constants.cardCanvas);

            var hoverCardGO = GameObject.Instantiate(
                cardPrefab,
                new Vector3(10000,10000, 0),
                Quaternion.identity
            ) as GameObject;
            hoverCardGO.transform.SetParent(cardCanvas.transform, false);
            hoverCardGO.name = hoverName;
            hoverCardGO.tag = "HoverCard";

            //disable all colliders so you can't hover the hover
            foreach (var collider in hoverCardGO.GetComponentsInChildren<BoxCollider>())
            {
                collider.enabled = false;
            }
            foreach (var collider in hoverCardGO.GetComponentsInChildren<MeshCollider>())
            {
                collider.enabled = false;
            }

            //set up fake card model
            var hoverCardModel = new CardModel()
            {
                playerId = -1,
                gameObject = hoverCardGO
            };

            hoverCardView = hoverCardGO.AddComponent<CardView>();
            hoverCardView.card = hoverCardModel;

            hoverCardGO.SetActive(false);
        }

        void Update()
        {
            if(!active) return;

            timer += Time.deltaTime;

            if (timer > CardView.HOVER_DELAY && cardVisible)
            {
                hoverCardView.gameObject.SetActive(true);
            }
        }

        //Should the hover timer be ticking?
        internal void setActive(bool newActive)
        {
            active = newActive;
        }

        internal void showCard(Vector3 position)
        {
            hoverCardView.UpdateText();

            hoverCardView.rectTransform.anchoredPosition3D = position;

            //hide card so it reshows after the delay
            hideCard();
            timer = 0f;
            cardVisible = true;
        }

        internal void showCardFromHand(CardModel cardToShow, Vector3 position)
        {
            //copy over props from hovered to hover
            cardToShow.CopyProperties(hoverCardView.card);
            //but reset some key things
            hoverCardView.name = hoverName;
            hoverCardView.card.gameObject = hoverCardView.gameObject;

            hoverCardView.rectTransform.SetAnchor(cardAnchor);
            var displayPosition = new Vector3(position.x, 125f, 19f);
            showCard(displayPosition);
        }

        internal void showPieceCardWorld(PieceModel piece, Vector3 worldPosition)
        {
            pieceService.CopyPieceToCard(piece, hoverCardView.card);
            hoverCardView.card.linkedPiece = piece;

            hoverCardView.UpdateBuffsDisplay();

            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

            var hWidth = hoverCardView.rectTransform.sizeDelta;
            hoverCardView.rectTransform.SetAnchor(bottomLeftAnchor);
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
                    showCard(screenPos + offset);
                    break;
                }
            }

            if (!cardVisible)
            {
                debug.LogWarning("No where to show hover card");
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
            cardVisible = false;
            //hoverCardView.card.linkedPiece = null;
            hoverCardView.gameObject.SetActive(false);
        }
    }
}


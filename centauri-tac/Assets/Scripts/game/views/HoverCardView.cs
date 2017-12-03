using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using ctac.signals;

namespace ctac
{
    public class HoverCardView : View
    {
        [Inject] public ICardService cardService { get; set; }
        [Inject] public IDebugService debug { get; set; }
        [Inject] public IPieceService pieceService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }
        [Inject] public CardHoverTipSignal cardHoverTipSignal { get; set; }

        public GameObject cardCanvas = null;

        float timer = 0f;
        bool cardVisible = false;
        bool active = true;
        float miniCardzPos = -5f;

        private string hoverName = "Hover Card";
        private CardView hoverCardView = null;
        private Canvas canvas { get; set; }

        private Vector2 cardAnchor = new Vector2(0.5f, 0);
        private Vector2 miniCardAnchor = new Vector2(1f, 0.5f);

        internal void init()
        {
            cardCanvas = (cardCanvas != null && cardCanvas.transform != null) ? cardCanvas : GameObject.Find(Constants.cardCanvas);
            canvas = GameObject.Find("Canvas").gameObject.GetComponent<Canvas>();
            //set up fake card model
            var hoverCardModel = new CardModel()
            {
                playerId = -1,
            };

            //init the hover card that's hidden most of the time
            cardService.CreateCard(hoverCardModel, cardCanvas.transform, new Vector3(10000,10000, 0));
            var hoverCardGO = hoverCardModel.gameObject;

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

            hoverCardView = hoverCardGO.AddComponent<CardView>();
            hoverCardView.card = hoverCardModel;

            hoverCardGO.SetActive(false);
        }

        void Update()
        {
            if(!active) return;

            timer += Time.deltaTime;

            if (timer > Constants.hoverTime && cardVisible)
            {
                hoverCardView.gameObject.SetActive(true);
            }
        }

        //Should the hover timer be ticking?
        internal void setActive(bool newActive)
        {
            active = newActive;
        }

        internal void showCard(Vector3 position, int spellDamage)
        {
            hoverCardView.UpdateText(spellDamage);
            cardService.UpdateCardArt(hoverCardView.card);

            hoverCardView.rectTransform.anchoredPosition3D = position;

            //hide card so it reshows after the delay
            hideCard();
            timer = 0f;
            cardVisible = true;
            cardHoverTipSignal.Dispatch(hoverCardView);
        }

        internal void hideCard()
        {
            cardVisible = false;
            timer = 0f;
            //hoverCardView.card.linkedPiece = null;
            hoverCardView.gameObject.SetActive(false);
            cardHoverTipSignal.Dispatch(null);
        }

        internal void showCardFromHand(CardModel cardToShow, Vector3 position, int spellDamage)
        {
            //copy over props from hovered to hover
            cardToShow.CopyProperties(hoverCardView.card);
            //but reset some key things
            hoverCardView.name = hoverName;
            hoverCardView.card.gameObject = hoverCardView.gameObject;

            hoverCardView.rectTransform.SetAnchor(cardAnchor);
            var displayPosition = new Vector3(position.x, 90f, Constants.cardHoverZPos);
            showCard(displayPosition, spellDamage);
        }

        internal void showMiniCardFromDeck(CardModel cardToShow, Vector3 position, int spellDamage)
        {
            //copy over props from hovered to hover
            cardToShow.CopyProperties(hoverCardView.card);
            //but reset some key things
            hoverCardView.name = hoverName;
            hoverCardView.card.gameObject = hoverCardView.gameObject;

            hoverCardView.rectTransform.SetAnchor(miniCardAnchor);
            var yPos = Mathf.Clamp(position.y, -76, 76);
            var displayPosition = new Vector3(position.x - 385f, yPos, miniCardzPos);
            showCard(displayPosition, spellDamage);
        }

        internal void showPieceCardWorld(PieceModel piece, Vector3 worldPosition, int spellDamage)
        {
            pieceService.CopyPieceToCard(piece, hoverCardView.card, true);

            hoverCardView.UpdateBuffsDisplay();

            hoverCardView.rectTransform.SetAnchor(Constants.topLeftAnchor);
            var hWidth = hoverCardView.rectTransform.sizeDelta;
            var position = new Vector2(hWidth.x / 2, -hWidth.y / 2) + (Constants.topLeftCardOffset * canvas.scaleFactor);

            showCard(new Vector3(position.x, position.y, Constants.cardHoverZPos), spellDamage);
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
    }
}


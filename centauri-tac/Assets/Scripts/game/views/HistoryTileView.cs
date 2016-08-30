using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ctac
{
    public class HistoryTileView : View, IPointerEnterHandler, IPointerExitHandler
    {
        [Inject]
        public IResourceLoaderService loader { get; set; }

        private CardView hoveringCard;
        private RectTransform rectTransform;
        private CardCanvasHelperView cardCanvasHelper;

        public HistoryItem item { get; set; }

        protected override void Start()
        {
            base.Start();
            rectTransform = GetComponent<RectTransform>();
            cardCanvasHelper = GameObject.Find(Constants.cardCanvas).GetComponent<CardCanvasHelperView>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item != null && item.cardsAffected.Count > 0)
            {
                hoveringCard = showCard(item.cardsAffected[0], transform.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoveringCard != null)
            {
                Destroy(hoveringCard.card.gameObject);
                hoveringCard = null;
            }
        }

        internal CardView showCard(CardModel cardToShow, Vector3 position)
        {
            var hoverCardView = CreateHoverCard();

            var hoverGo = hoverCardView.card.gameObject;

            //copy over props from hovered to hover
            cardToShow.CopyProperties(hoverCardView.card);
            hoverCardView.card.gameObject = hoverGo;

            hoverCardView.UpdateText();

            //now for the fun positioning

            Vector2 viewportPos = Camera.main.WorldToViewportPoint(position);
            Vector2 cardCanvasPos = cardCanvasHelper.ViewportToWorld(viewportPos);
            var hWidth = hoverCardView.rectTransform.sizeDelta;
            var displayPosition = new Vector3(cardCanvasPos.x + hWidth.x, cardCanvasPos.y + (hWidth.y * 0.75f), position.z);

            hoverCardView.rectTransform.SetAnchor(Vector2.zero);
            hoverCardView.rectTransform.anchoredPosition3D = displayPosition;
            
            return hoverCardView;
        }

        private CardView CreateHoverCard()
        {
            var cardPrefab = loader.Load<GameObject>("Card");
            var cardCanvas = GameObject.Find(Constants.cardCanvas);

            var hoverCardGO = GameObject.Instantiate(
                cardPrefab,
                new Vector3(10000,10000, 0),
                Quaternion.identity
            ) as GameObject;
            hoverCardGO.transform.SetParent(cardCanvas.transform, false);

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

            var hoverCardView = hoverCardGO.AddComponent<CardView>();
            hoverCardView.card = hoverCardModel;
            
            return hoverCardView;
        }
    }
}


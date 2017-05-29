using ctac.signals;
using ctac.util;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac {
    public class CardsView : View
    {
        //should only be the current players cards
        private List<CardModel> playerCards { get; set; }
        private List<CardModel> opponentCards { get; set; }

        private CardSelectedModel selectedCard { get; set; }
        private bool selectedNeedsArrow { get; set; }
        private CardModel hoveredCard { get; set; }
        private CardCanvasHelperView cardCanvasHelper;
        private Canvas canvas { get; set; }

        private Vector2 anchorPosition = new Vector2(0.5f, 0);
        private Vector2 opponentAnchorPosition = new Vector2(0.5f, 1);
        private const float maxDragDistance = 100f;
        private Vector3 dest;

        //private Vector2 cardDimensions = new Vector2(156, 258.2f);
        private Material cardGlowMat = null;
        private Material cardOutlineMat = null;

        private float hoverAccumulator = 0f;
        private Vector3 cardCircleCenter = new Vector3(0, -590, 332);
        private Vector3 opponentCardCircleCenter = new Vector3(0, 750, 332);
        private float cardCircleRadius = 480f;
        private float cardAngleSpread = -5f;

        protected override void Start()
        {
        }

        public void init(List<CardModel> playerCards, List<CardModel> opponentCards)
        {
            this.playerCards = playerCards;
            this.opponentCards = opponentCards;
            cardCanvasHelper = GameObject.Find(Constants.cardCanvas).GetComponent<CardCanvasHelperView>();
            canvas = GameObject.Find("Canvas").gameObject.GetComponent<Canvas>();
        }

        void Update()
        {
            if(playerCards == null || opponentCards == null) return;

            //set up card cost glowing material first time
            if (cardGlowMat == null && playerCards.Count > 1)
            {
                cardOutlineMat = playerCards[0].cardView.costText.fontSharedMaterial;
                cardGlowMat = new Material(cardOutlineMat);
                cardGlowMat.SetFloat(ShaderUtilities.ID_GlowPower, 0.4f);
                cardGlowMat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.0f);
            }

            //position opponents cards
            //might need to DRY it up sometime but rule of three still holds
            cardAngleSpread = -13f + (0.9f * opponentCards.Count);
            for(int c = 0; c < opponentCards.Count; c++) 
            {
                var card = opponentCards[c];
                if(card.activated) continue;
                var rectTransform = card.rectTransform;
                var cardCountOffset = 0 - ((opponentCards.Count - 1) / 2) + c;
                rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f + cardCountOffset * cardAngleSpread));
                rectTransform.Rotate(Vector3.up, 180f, Space.Self);

                dest = PointOnCircle(cardCircleRadius, 270f + cardCountOffset * cardAngleSpread, opponentCardCircleCenter);
                dest = dest.SetZ(dest.z + (-1.0f * c));

                rectTransform.anchorMax = opponentAnchorPosition;
                rectTransform.anchorMin = opponentAnchorPosition;
                rectTransform.pivot = opponentAnchorPosition;
                rectTransform.anchoredPosition3D = iTween.Vector3Update(rectTransform.anchoredPosition3D, dest, 10.0f);
            }

            //and now players cards
            cardAngleSpread = -16f + (1.1f * playerCards.Count);
            for(int c = 0; c < playerCards.Count; c++) 
            {
                var card = playerCards[c];
                if(card.activated) continue;
                var rectTransform = card.rectTransform;
                var cardCountOffset = 0 - ((playerCards.Count - 1) / 2) + c;
                rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, cardCountOffset * cardAngleSpread));

                dest = PointOnCircle(cardCircleRadius, 90f + cardCountOffset * cardAngleSpread, cardCircleCenter);
                dest = dest.SetZ(dest.z + (-1.0f * c));

                //drag the selected card with the cursor
                if (selectedCard != null && card.id == selectedCard.card.id)
                {
                    var mousePos = CrossPlatformInputManager.mousePosition;
                    var mouseBefore = new Vector3(mousePos.x, mousePos.y, 130);
                    var mouseWorld = cardCanvasHelper.CardCameraToWorld(mouseBefore);


                    var dragDist = Vector2.Distance(mouseWorld, dest);
                    if (dragDist < maxDragDistance || !selectedNeedsArrow)
                    {
                        //Actually drag the card around the screen for spells
                        rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        rectTransform.position = mouseWorld;
                        continue;
                    }
                    else
                    {
                        //for targeted spells and minions, just bump it up in the hand to show the selected card
                        rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        dest = new Vector3(dest.x, dest.y + 60f, dest.z - 15f);
                    }
                }
                //show the hover card on top of where the actual card is after a delay
                if (hoveredCard != null && card == hoveredCard && (selectedCard == null || hoveredCard != selectedCard.card))
                {
                    dest = dest.SetY(dest.y + 30f);
                    hoverAccumulator += Time.deltaTime;

                    if (hoverAccumulator > CardView.HOVER_DELAY)
                    {
                        card.cardView.displayWrapper.SetActive(false);
                    }
                }

                rectTransform.anchorMax = anchorPosition;
                rectTransform.anchorMin = anchorPosition;
                rectTransform.pivot = anchorPosition;
                rectTransform.anchoredPosition3D = iTween.Vector3Update(rectTransform.anchoredPosition3D, dest, 10.0f);

                if (card.playable && cardGlowMat != null)
                {
                    card.cardView.costText.fontMaterial = cardGlowMat;
                }
                else if(cardOutlineMat != null)
                {
                    card.cardView.costText.fontMaterial = cardOutlineMat;
                }
            }
        }

        private static Vector3 PointOnCircle(float radius, float angleInDegrees, Vector3 origin)
        {
            // Convert from degrees to radians via multiplication by PI/180
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F)) + origin.x;
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F)) + origin.y;

            return new Vector3(x, y, origin.z);
        }



        internal void onCardSelected(CardSelectedModel card, bool needsArrow)
        {
            selectedCard = card;
            selectedNeedsArrow = needsArrow;
            if (hoveredCard != null)
            {
                hoveredCard.cardView.displayWrapper.SetActive(true);
            }
        }

        internal void onCardHovered(CardModel card)
        {
            //reset previous hovered card to active
            if (hoveredCard != null)
            {
                hoveredCard.cardView.displayWrapper.SetActive(true);
            }

            //then update
            hoveredCard = card;
            hoverAccumulator = 0f;
        }

        public class CardDestroyedAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public CardDestroyedSignal cardDestroyed { get; set; }
            public CardModel card { get; set; }

            public void Init() { }
            public void Update()
            {
                iTweenExtensions.ScaleTo(card.gameObject, Vector3.zero, 0.5f, 0, EaseType.easeInCubic);
                if (card.gameObject.transform.localScale.x < 0.01f)
                {
                    card.gameObject.transform.localScale = Vector3.zero;
                    Complete = true;
                    cardDestroyed.Dispatch(card);
                }
            }
        }

        public class DrawCardAnim : IAnimate
        {
            public ISoundService sounds { get; set; }
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return 0.2f; } }

            public CardDrawShownSignal cardDrawn { get; set; }
            public CardModel card { get; set; }
            public bool isOpponentCard { get; set; }

            //dev speed
            private float animTime = 0.2f;
            private Vector3 opponentDest = new Vector3(0, 250, -12f);
            private Vector3 playerDest = new Vector3(0, 0, -12f);

            public void Init() {}
            public void Update()
            {
                Vector3 dest = playerDest;
                if (isOpponentCard)
                {
                    animTime = 0.3f;
                    dest = opponentDest;
                    iTweenExtensions.RotateTo(card.gameObject, new Vector3(0, 180, 180), animTime, 0, EaseType.easeOutCubic);
                }
                else
                {
                    iTweenExtensions.RotateTo(card.gameObject, Vector3.zero, animTime, 0, EaseType.easeOutCubic);
                }
                iTweenExtensions.MoveToLocal(card.gameObject, dest, animTime, 0, EaseType.easeOutCubic);
                if (Vector3.Distance(card.gameObject.transform.localPosition, dest) < 0.08f)
                {
                    if (!isOpponentCard)
                    {
                        sounds.PlaySound("drawCard");
                    }
                    card.gameObject.transform.localPosition = dest;
                    Complete = true;
                    cardDrawn.Dispatch(card);
                }
            }
        }

        public class GiveCardAnim : IAnimate
        {
            private float animTime = 0.5f;

            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return animTime; } }

            public CardModel card { get; set; }
            public bool isOpponentCard { get; set; }

            private RectTransform rectTransform;

            public void Init() {
                var midScreen = new Vector3(0.5f, 0.5f, 0);

                rectTransform = card.rectTransform;
                rectTransform.anchorMax = midScreen;
                rectTransform.anchorMin = midScreen;
                rectTransform.anchoredPosition3D = midScreen;
                if (isOpponentCard)
                {
                    rectTransform.localRotation = Quaternion.Euler(new Vector3(0, 180, 180));
                }
                else
                {
                    rectTransform.localRotation = Quaternion.identity;
                }
                rectTransform.localScale = Vector3.zero;
                rectTransform.pivot = midScreen;
            }
            public void Update()
            {
                iTweenExtensions.ScaleTo(card.gameObject, Vector3.one, animTime, 0f, EaseType.easeInOutQuint);
                Complete = true;
            }
        }

        public class DiscardCardAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return 0.5f; } }

            public DestroyCardSignal destroyCard { get; set; }
            public CardModel card { get; set; }
            public bool isOpponentCard { get; set; }

            //dev speed
            private float animTime = 0.5f;
            private Vector3 opponentDest = new Vector3(0, 150, 50);

            private float elapsedTime = 0f;

            public void Init() { }
            public void Update()
            {
                elapsedTime += Time.deltaTime;
                Vector3 dest = Vector3.zero;
                if (isOpponentCard)
                {
                    animTime = 0.3f;
                    dest = opponentDest;
                }
                iTweenExtensions.RotateTo(card.gameObject, Vector3.zero, animTime, 0, EaseType.easeOutCubic);

                iTweenExtensions.MoveToLocal(card.gameObject, dest, animTime, 0, EaseType.easeOutCubic);
                if (elapsedTime > animTime)
                {
                    Complete = true;
                    destroyCard.Dispatch(card.id);
                }
            }
        }

        public class OverdrawCardAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return false; } }
            public float? postDelay { get { return 0.5f; } }

            public AnimationQueueModel animationQueue { get; set; }
            public CardDestroyedSignal cardDestroyed { get; set; }
            public CardModel card { get; set; }

            //dev speed
            private float animTime = 0.5f;
            private Vector3 dest = new Vector3(250, 0, 0);

            public void Init() { }
            public void Update()
            {
                iTweenExtensions.RotateTo(card.gameObject, Vector3.zero, animTime, 0, EaseType.easeOutCubic);
                iTweenExtensions.MoveToLocal(card.gameObject, dest, animTime, 0, EaseType.easeOutCubic);
                if (Vector3.Distance(card.gameObject.transform.localPosition, dest) < 0.08f)
                {
                    card.gameObject.transform.localPosition = dest;
                    animationQueue.Add(new CardDestroyedAnim()
                    {
                        card = card,
                        cardDestroyed = cardDestroyed
                    });
                    Complete = true;
                }
            }
        }

    }
}

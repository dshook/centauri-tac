using ctac.signals;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

namespace ctac
{
    public class HistoryTileView : View, IPointerEnterHandler, IPointerExitHandler
    {
        [Inject]
        public IResourceLoaderService loader { get; set; }
        [Inject]
        public IPieceService pieceService { get; set; }
        [Inject]
        public IDebugService debug { get; set; }

        private CardCanvasHelperView cardCanvasHelper;
        //All the currently hovering cards indexed by either the piece or card Id they're for.  Piece Id's have their sign flipped
        //so the Id's don't collied with cards
        private Dictionary<int, CardView> hoveringCards = new Dictionary<int, CardView>();
        //same structure for how many change things we put on top of the card we can use for positioning
        private Dictionary<int, int> changeCounts = new Dictionary<int, int>();

        public HistoryItem item { get; set; }
        private bool hovering = false;

        [Inject] public HistoryHoverSignal historyHoverSignal { get; set; }


        protected override void Start()
        {
            base.Start();
            cardCanvasHelper = GameObject.Find(Constants.cardCanvas).GetComponent<CardCanvasHelperView>();
        }

        void Update()
        {
            const float scrollAmt = 20f;
            if (hovering && hoveringCards.Count > 1)
            {
                bool scrollLeft = false;
                bool scrollRight = false;
                if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") > 0)
                {
                    scrollRight = true;
                }
                if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") < 0)
                {
                    scrollLeft = true;
                }
                if (scrollLeft || scrollRight)
                {
                    foreach (var key in hoveringCards)
                    {
                        //skip the anchoring hovered card for the triggering piece or card
                        if (
                            (item.triggeringCard != null && key.Key == item.triggeringCard.id)
                            || (item.triggeringPiece != null && key.Key == -item.triggeringPiece.id)
                        ) { continue; }

                        hoveringCards[key.Key].transform.localPosition = hoveringCards[key.Key].transform.localPosition.AddX(scrollLeft ? -scrollAmt : scrollAmt);
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item != null)
            {
                hovering = true;
                historyHoverSignal.Dispatch(true);
                //show main card/piece as a card
                if (item.triggeringCard != null)
                {
                    hoveringCards[item.triggeringCard.id] = showCard(item.triggeringCard, Vector3.zero, item.spellDamage);
                }
                else if (item.triggeringPiece != null)
                {
                    var pieceCard = new CardModel();
                    pieceService.CopyPieceToCard(item.triggeringPiece, pieceCard, true);
                    hoveringCards[-item.triggeringPiece.id] = showCard(pieceCard, Vector3.zero, item.spellDamage);
                }

                //show all damage/heal events
                var xOffset = 180f;
                var numberSplat = loader.Load<GameObject>("NumberSplat");
                var deathOverlay = loader.Load<GameObject>("DeathOverlay");
                for (int i = 0; i < item.pieceChanges.Count; i++)
                {
                    var pieceChange = item.pieceChanges[i];
                    changeCounts[pieceChange.originalPiece.id] = 
                        changeCounts.ContainsKey(pieceChange.originalPiece.id) 
                        ? changeCounts[pieceChange.originalPiece.id] + 1 
                        : 1;
                    var pieceChangeCount = changeCounts[pieceChange.originalPiece.id];
                    var totalPieceChangeCount = item.pieceChanges.Where(p => p.originalPiece.id == pieceChange.originalPiece.id).Count();

                    //gonna cap out the piece changes we can show so we don't end up with horribly broken UI in rare cases
                    if (pieceChangeCount > 6)
                    {
                        continue;
                    }

                    CardView healthChangeCard = null;
                    //if the hp change is about an existing hovered card, reuse that card instead of making a new one
                    if (pieceChange.originalPiece != null && hoveringCards.ContainsKey(-pieceChange.originalPiece.id))
                    {
                        healthChangeCard = hoveringCards[-pieceChange.originalPiece.id];
                    }
                    else
                    {
                        var pieceCard = new CardModel();
                        pieceService.CopyPieceToCard(pieceChange.originalPiece, pieceCard, true);
                        healthChangeCard = showCard(pieceCard, new Vector3(xOffset * (i + 1), 0, 0), 0);

                        hoveringCards[-pieceChange.originalPiece.id] = healthChangeCard;
                    }

                    if (pieceChange.type == HistoryPieceChangeType.HealthChange)
                    {
                        var hpChange = (HistoryHealthChange)pieceChange;
                        //death vs damage
                        if (pieceChange.originalPiece.health <= 0)
                        {
                            var deathSplat = Instantiate(deathOverlay);
                            deathSplat.transform.SetParent(healthChangeCard.displayWrapper.transform, false);
                        }
                        else
                        {
                            //plop down the number splat scaled up and reset to the right position
                            var newNumberSplat = Instantiate(numberSplat, healthChangeCard.displayWrapper.transform, false) as GameObject;
                            var yPos = totalPieceChangeCount == 1 ? 40.5f : 120 - (pieceChangeCount * 40f);
                            newNumberSplat.transform.localPosition = new Vector3(0, yPos, -2.0f);
                            newNumberSplat.transform.localScale = new Vector3(140, 140, 1);
                            newNumberSplat.transform.localRotation = Quaternion.Euler(Vector3.zero);
                            var view = newNumberSplat.GetComponent<NumberSplatView>();
                            view.change = hpChange.healthChange.change;
                            view.bonus = hpChange.healthChange.bonus;
                            view.bonusText = hpChange.healthChange.bonusMsg;
                            view.animate = false;
                        }
                    }
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            historyHoverSignal.Dispatch(false);
            hovering = false;
            if (hoveringCards.Count > 0)
            {
                foreach(var key in hoveringCards) 
                {
                    Destroy(hoveringCards[key.Key].card.gameObject);
                }
                hoveringCards.Clear();
            }
            changeCounts.Clear();
        }

        internal CardView showCard(CardModel cardToShow, Vector3 positionOffset, int spellDamage)
        {
            var selfPos = transform.position;
            var hoverCardView = CreateHoverCard();

            var hoverGo = hoverCardView.card.gameObject;

            //copy over props from hovered to hover
            cardToShow.CopyProperties(hoverCardView.card);
            hoverCardView.card.gameObject = hoverGo;
            hoverCardView.staticSpellDamage = spellDamage;
            hoverGo.name = "Hover Card for " + hoverCardView.card.id;

            hoverCardView.UpdateText(spellDamage);
            hoverCardView.UpdateBuffsDisplay();

            //now for the fun positioning

            Vector2 viewportPos = Camera.main.WorldToViewportPoint(selfPos);
            Vector2 cardCanvasPos = cardCanvasHelper.ViewportToWorld(viewportPos);
            var hWidth = hoverCardView.rectTransform.sizeDelta;
            var displayPosition = new Vector3(cardCanvasPos.x + hWidth.x, cardCanvasPos.y + (hWidth.y * 0.75f), -5f);
            displayPosition = displayPosition + positionOffset;

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


using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections.Generic;
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

        private CardCanvasHelperView cardCanvasHelper;
        private List<CardView> hoveringCards = new List<CardView>();

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
                if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") > 0)
                {
                    for (int c = 1; c < hoveringCards.Count; c++)
                    {
                        hoveringCards[c].transform.localPosition = hoveringCards[c].transform.localPosition.AddX(scrollAmt);
                    }
                }
                if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") < 0)
                {
                    for (int c = 1; c < hoveringCards.Count; c++)
                    {
                        hoveringCards[c].transform.localPosition = hoveringCards[c].transform.localPosition.AddX(-scrollAmt);
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
                    hoveringCards.Add(showCard(item.triggeringCard, Vector3.zero, item.spellDamage));
                }
                else if (item.triggeringPiece != null)
                {
                    var pieceCard = new CardModel();
                    pieceService.CopyPieceToCard(item.triggeringPiece, pieceCard);
                    hoveringCards.Add(showCard(pieceCard, Vector3.zero, item.spellDamage));
                }

                //show all damage/heal events
                var xOffset = 180f;
                var damageSplat = loader.Load<GameObject>("DamageSplat");
                var deathOverlay = loader.Load<GameObject>("DeathOverlay");
                for (int i = 0; i < item.pieceChanges.Count; i++)
                {
                    var pieceChange = item.pieceChanges[i];

                    CardView healthChangeCard = null;
                    //if the hp change is about the triggering piece reuse that card instead of making a new one
                    if (item.triggeringPiece != null && pieceChange.originalPiece.id == item.triggeringPiece.id)
                    {
                        healthChangeCard = hoveringCards[0];
                    }
                    else
                    {
                        var pieceCard = new CardModel();
                        pieceService.CopyPieceToCard(pieceChange.originalPiece, pieceCard);
                        healthChangeCard = showCard(pieceCard, new Vector3(xOffset * (i + 1), 0, 0), 0);
                        hoveringCards.Add(healthChangeCard);
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
                            var dmgSplat = Instantiate(damageSplat);
                            dmgSplat.transform.SetParent(healthChangeCard.displayWrapper.transform, false);
                            var text = dmgSplat.transform.FindChild("Text").GetComponent<TextMeshPro>();
                            var bonusText = dmgSplat.transform.FindChild("Bonus").GetComponent<TextMeshPro>();

                            text.text = hpChange.healthChange.change.ToString();
                            bonusText.text = hpChange.healthChange.bonus + " " + hpChange.healthChange.bonusMsg;
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
                for (int i = 0; i < hoveringCards.Count; i++)
                {
                    Destroy(hoveringCards[i].card.gameObject);
                }
                hoveringCards.Clear();
            }
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


using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;
using UnityEngine;
using System.Linq;
using System;

namespace ctac
{
    public class CardsMenuView : View
    {
        public Signal clickLeaveSignal = new Signal();
        public Signal<Races> clickNewDeckSignal = new Signal<Races>();
        public Signal<DeckModel> clickSaveDeckSignal = new Signal<DeckModel>();
        public Signal clickCancelDeckSignal = new Signal();

        public Button leaveButton;
        public Button prevButton;
        public Button nextButton;
        public Button newDeckButton;
        public Button saveDeckButton;
        public Button cancelDeckButton;
        public Slider energySlider;
        public TMP_InputField searchBox;

        public Toggle venusiansToggle;
        public Toggle earthlingsToggle;
        public Toggle martiansToggle;
        public Toggle grexToggle;
        public Toggle phaenonToggle;
        public Toggle lostToggle;
        public Toggle neutralToggle;

        Dictionary<Races, Toggle> raceToggles;

        public Button venusiansButton;
        public Button earthlingsButton;
        public Button martiansButton;
        public Button grexButton;
        public Button phaenonButton;
        public Button lostButton;

        ICardService cardService;
        CardDirectory cardDirectory;

        public GameObject cardHolder;
        public GameObject cardControls;
        public GameObject factionSelection;
        public GameObject deckHolder;
        public GameObject deckEditHolder;
        
        const int pageSize = 8;
        const int rowSize = 4;
        const int cardBufferSize = pageSize * 2;
        Vector2 cardAnchor = new Vector2(0, 1);

        List<CardModel> createdCards = new List<CardModel>();

        int offset = 0;
        int prevOffset = -1;
        int remainingCardsToShow = 0;
        int? energyFilter = null;
        string stringFilter = null;
        Dictionary<Races, bool> raceFilters;

        DeckModel editingDeck = null;

        internal void init(ICardService cs, CardDirectory cd)
        {
            cardService = cs;
            cardDirectory = cd;

            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
            newDeckButton.onClick.AddListener(onNewDeckClick);
            saveDeckButton.onClick.AddListener(onSaveDeckClick);
            cancelDeckButton.onClick.AddListener(onCancelDeckClick);

            venusiansButton.onClick.AddListener(() => onFactionSelect(Races.Venusians));
            earthlingsButton.onClick.AddListener(() => onFactionSelect(Races.Earthlings));
            martiansButton.onClick.AddListener(() => onFactionSelect(Races.Martians));
            grexButton.onClick.AddListener(() => onFactionSelect(Races.Grex));
            phaenonButton.onClick.AddListener(() => onFactionSelect(Races.Phaenon));
            lostButton.onClick.AddListener(() => onFactionSelect(Races.Lost));

            prevButton.onClick.AddListener(onPrevButton);
            nextButton.onClick.AddListener(onNextButton);
            energySlider.onValueChanged.AddListener(onEnergySlider);
            searchBox.onValueChanged.AddListener(onSearchChange);

            cardHolder.transform.DestroyChildren(true);

            //manually wire this up for now
            raceFilters = new Dictionary<Races, bool>();
            raceToggles = new Dictionary<Races, Toggle>()
            {
                {Races.Venusians, venusiansToggle },
                {Races.Earthlings, earthlingsToggle },
                {Races.Martians, martiansToggle },
                {Races.Grex, grexToggle },
                {Races.Phaenon, phaenonToggle },
                {Races.Lost, lostToggle },
                {Races.Neutral, neutralToggle },
            };
            foreach (var toggle in raceToggles)
            {
                toggle.Value.isOn = false;
                toggle.Value.interactable = true;
                toggle.Value.onValueChanged.AddListener((value) => onToggleSelect(value, toggle.Key));
                raceFilters.Add(toggle.Key, false);

            }

            //create all the card game objects that will be recycled
            for (var c = 0; c < cardBufferSize; c++)
            {
                var cardModel = new CardModel() { playerId = 0, };

                cardService.CreateCard(cardModel, cardHolder.transform);

                createdCards.Add(cardModel);
            }
        }

        void Update()
        {
        }

        //OPTIMIZATION: the filtering of cards could be separated from the paging of cards so you don't refilter each time you page
        internal void UpdateCards(bool resetPage = false)
        {
            if (resetPage)
            {
                offset = 0;
            }
            bool isForward = offset > prevOffset;
            prevOffset = offset;
            var allowAllRaces = !raceFilters.Any(c => c.Value == true);

            var cardList = cardDirectory.directory
                .Where(c => !c.uncollectible && !c.isHero)
                .Where(c => !energyFilter.HasValue || c.cost == energyFilter.Value)
                .Where(c => String.IsNullOrEmpty(stringFilter)
                    || c.name.ToLower().Contains(stringFilter)
                    || c.description.ToLower().Contains(stringFilter)
                    || c.tags.Any(t => t.ToLower().Contains(stringFilter))
                )
                .Where(c => allowAllRaces || raceFilters[c.race]);

            remainingCardsToShow = cardList.Count() - offset - pageSize;

            DisplayCards(cardList.Skip(offset).Take(pageSize).ToList(), isForward);
        }

        //Should just be the 8 cards to display
        int createdCardOffset = 0;
        void DisplayCards(List<CardModel> cards, bool isForward)
        {
            float cardDist = 1200;
            //float animTime = 1f;
            Vector3 animDestPosition  = new Vector3(isForward ? -cardDist : cardDist, 0, 0);
            Vector3 animStartPosition = new Vector3(isForward ? cardDist : -cardDist, 0, 0);

            //animate all existing cards out depending on if it's forward or backward
            for (int c = 0; c < cardHolder.transform.childCount; c++)
            {
                var childCard = cardHolder.transform.GetChild(c);
                //iTweenExtensions.MoveToLocal(childCard.gameObject, childCard.transform.position + animDestPosition, animTime, 0f);
                var rectTrans = childCard.GetComponent<RectTransform>();
                rectTrans.anchoredPosition3D = rectTrans.anchoredPosition3D + animDestPosition;
            }

            //now copy all the card props to the surrogate cards and animate them in
            for (int c = 0; c < cards.Count; c++)
            {
                var card = cards[c];
                var surrogateCard = createdCards[c + createdCardOffset];
                var cardGameObject = surrogateCard.gameObject;

                //copy over props from hovered to hover
                card.CopyProperties(surrogateCard);
                //but reset some key things
                surrogateCard.gameObject = cardGameObject;

                //Set up all this reference stuff from the directory card model to the surrogate card that's displaying it
                var cardView = cardGameObject.GetComponent<CardView>();
                if (cardView == null)
                {
                    cardView = cardGameObject.AddComponent<CardView>();
                }
                cardView.card = surrogateCard;
                surrogateCard.rectTransform = cardGameObject.GetComponent<RectTransform>();
                surrogateCard.cardView = cardView;
                cardService.UpdateCardArt(surrogateCard);
                surrogateCard.cardView.UpdateText(0);

                //now position to the grid
                surrogateCard.gameObject.transform.SetParent(cardHolder.transform);
                var xPos = 195 * (c % rowSize);
                var yPos = c >= rowSize ? -419 : -135;

                surrogateCard.rectTransform.anchorMax = cardAnchor;
                surrogateCard.rectTransform.anchorMin = cardAnchor;
                surrogateCard.rectTransform.pivot = cardAnchor;
                var destPosition = new Vector3(xPos, yPos);
                surrogateCard.rectTransform.anchoredPosition3D = animStartPosition + destPosition;
                surrogateCard.rectTransform.anchoredPosition3D = destPosition;
                surrogateCard.gameObject.SetActive(true);
                //iTweenExtensions.MoveToLocal(card.gameObject, destPosition, animTime, 0f);
            }

            //now disable any straggling cards for this page, like if there's only 1 card on the page disable the other 7
            for (int c = cards.Count + createdCardOffset; c < pageSize + createdCardOffset; c++)
            {
                createdCards[c].gameObject.SetActive(false);
            }

            //swip swap the next cards to use by either incrimenting or resetting the created card offset
            createdCardOffset = createdCardOffset == 0 ? pageSize : 0;
        }

        void onPrevButton()
        {
            offset -= pageSize;
            offset = Math.Max(0, offset);
            if (offset == prevOffset) return; //skip work we don't need to do
            UpdateCards();
        }

        void onNextButton()
        {
            //don't need to do anything when we've run out of cards to show with the filters
            if (remainingCardsToShow <= 0) return;

            offset += pageSize;
            if (offset == prevOffset) return; //skip work we don't need to do
            UpdateCards();
        }

        void onEnergySlider(float value)
        {
            if (value == -1)
            {
                energyFilter = null;
            }
            else
            {
                energyFilter = (int)value;
            }
            //reset what page we're on when filtering
            UpdateCards(true);
        }

        void onSearchChange(string value)
        {
            stringFilter = value.ToLower();
            UpdateCards(true);
        }

        void onNewDeckClick()
        {
            //swip swap the cards showing for the race selection buttons
            ShowRaceSelectionButtons(true);
        }

        void onFactionSelect(Races race)
        {
            ShowRaceSelectionButtons(false);
            clickNewDeckSignal.Dispatch(race);
        }

        void onToggleSelect(bool status, Races race)
        {
            raceFilters[race] = status;
            UpdateCards(true);
        }

        internal void onEditDeck(DeckModel deck)
        {
            editingDeck = deck;
            ShowDeckHolder(false);
            LimitRaceFilters(deck.race);
        }

        void onSaveDeckClick()
        {
            clickSaveDeckSignal.Dispatch(editingDeck);
        }

        internal void onDeckSaved()
        {
            ResetRaceFilters();
            editingDeck = null;
            ShowDeckHolder(true);
        }
        
        void onCancelDeckClick()
        {
            editingDeck = null;
            ShowDeckHolder(true);
            clickCancelDeckSignal.Dispatch();
        }

        //swap the cards out for faction selection buttons
        void ShowRaceSelectionButtons(bool show)
        {
            cardHolder.SetActive(!show);
            cardControls.SetActive(!show);
            factionSelection.SetActive(show);
        }

        //Show either deck holder, or the deck edit holder
        void ShowDeckHolder(bool show)
        {
            float showXPos = 5f;
            float hideXPos = 222f;
            var deckHolderRect = deckHolder.GetComponent<RectTransform>();
            var deckEditHolderRect = deckEditHolder.GetComponent<RectTransform>();
            if (show)
            {
                deckHolderRect.anchoredPosition3D = deckHolderRect.anchoredPosition3D.SetX(showXPos);
                deckEditHolderRect.anchoredPosition3D = deckEditHolderRect.anchoredPosition3D.SetX(hideXPos);
            }
            else
            {
                deckHolderRect.anchoredPosition3D = deckHolderRect.anchoredPosition3D.SetX(hideXPos);
                deckEditHolderRect.anchoredPosition3D = deckEditHolderRect.anchoredPosition3D.SetX(showXPos);
            }
        }

        //limit the faction selections to neutral and a particular race
        internal void LimitRaceFilters(Races race)
        {
            foreach (var toggle in raceToggles)
            {
                toggle.Value.isOn = false;
                toggle.Value.interactable = false;
            }

            raceToggles[race].isOn = true;
            raceToggles[race].interactable = true;

            raceToggles[Races.Neutral].interactable = true;
        }

        internal void ResetRaceFilters()
        {
            foreach (var toggle in raceToggles)
            {
                toggle.Value.isOn = false;
                toggle.Value.interactable = true;
            }
        }
    }
}


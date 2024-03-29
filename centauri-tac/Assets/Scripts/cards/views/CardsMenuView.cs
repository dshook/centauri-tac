using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;
using UnityEngine;
using System.Linq;
using System;
using System.Collections;

namespace ctac
{
    public class CardsMenuView : View
    {
        public Signal<Races> clickNewDeckSignal = new Signal<Races>();
        public Signal<DeckModel> clickSaveDeckSignal = new Signal<DeckModel>();
        public Signal<DeckModel> clickDeleteDeckSignal = new Signal<DeckModel>();


        public Button leaveButton;
        public Button prevButton;
        public Button nextButton;
        public Button newDeckButton;
        public Button saveDeckButton;
        public Button cancelDeckButton;
        public Button deleteDeckButton;
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

        public Toggle cardSetAllToggle;
        public Toggle cardSetBasicToggle;
        public Toggle cardSetTestToggle;

        Dictionary<CardSets, Toggle> cardSetToggles;

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

        DeckModel editingDeck = null;
        Races defaultRace = Races.Neutral;
        CardSets defaultCardSet = CardSets.none;

        internal void init(ICardService cs, CardDirectory cd)
        {
            cardService = cs;
            cardDirectory = cd;

            newDeckButton.onClick.AddListener(onNewDeckClick);
            saveDeckButton.onClick.AddListener(onSaveDeckClick);
            deleteDeckButton.onClick.AddListener(onDeleteDeckClick);

            venusiansButton.onClick.AddListener(() => onFactionSelect(Races.Vae));
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
            raceToggles = new Dictionary<Races, Toggle>()
            {
                {Races.Vae, venusiansToggle },
                {Races.Earthlings, earthlingsToggle },
                {Races.Martians, martiansToggle },
                {Races.Grex, grexToggle },
                {Races.Phaenon, phaenonToggle },
                {Races.Lost, lostToggle },
                {Races.Neutral, neutralToggle },
            };
            foreach (var toggle in raceToggles)
            {
                var startingFilterValue = false;
                if(toggle.Key == defaultRace){ startingFilterValue = true; }

                toggle.Value.isOn = startingFilterValue;
                toggle.Value.interactable = true;
                toggle.Value.onValueChanged.AddListener((value) => UpdateCards(true));
            }

            //more manual wiring
            cardSetToggles = new Dictionary<CardSets, Toggle>()
            {
                {CardSets.none, cardSetAllToggle },
                {CardSets.basic, cardSetBasicToggle },
                {CardSets.test, cardSetTestToggle },
            };
            foreach (var toggle in cardSetToggles)
            {
                var startingFilterValue = false;
                if(toggle.Key == defaultCardSet){ startingFilterValue = true; }

                toggle.Value.isOn = startingFilterValue;
                toggle.Value.interactable = true;
                toggle.Value.onValueChanged.AddListener((value) => UpdateCards(true));
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
            var allowAllRaces = !raceToggles.Any(c => c.Value.isOn == true);
            var allowAllSets = cardSetToggles[CardSets.none].isOn;

            var cardList = cardDirectory.directory
                .Where(c => !c.uncollectible && !c.isHero)
                .Where(c => !energyFilter.HasValue || c.cost == energyFilter.Value)
                .Where(c => String.IsNullOrEmpty(stringFilter)
                    || c.name.ToLower().Contains(stringFilter)
                    || c.description.ToLower().Contains(stringFilter)
                    || c.tags.Any(t => t.ToLower().Contains(stringFilter))
                )
                .Where(c => allowAllRaces || raceToggles[c.race].isOn)
                .Where(c => allowAllSets || cardSetToggles[c.cardSet].isOn)
                .OrderBy(c => c.cost)
                .ThenBy(c => c.name);

            remainingCardsToShow = cardList.Count() - offset - pageSize;

            DisplayCards(cardList.Skip(offset).Take(pageSize).ToList(), isForward);
        }

        //Should just be the 8 cards to display
        int createdCardOffset = 0;
        void DisplayCards(List<CardModel> cards, bool isForward)
        {
            //float cardDist = 1200;
            //float animTime = 1f;
            //Vector3 animDestPosition  = new Vector3(isForward ? -cardDist : cardDist, 0, 0);
            //Vector3 animStartPosition = new Vector3(isForward ? cardDist : -cardDist, 0, 0);

            //Inactivate all the current showing cards
            for (int c = 0; c < cardHolder.transform.childCount; c++)
            {
                var childCard = cardHolder.transform.GetChild(c);
                childCard.gameObject.SetActive(false);
                //iTweenExtensions.MoveToLocal(childCard.gameObject, childCard.transform.position + animDestPosition, animTime, 0f);
                //var rectTrans = childCard.GetComponent<RectTransform>();
                //rectTrans.anchoredPosition3D = rectTrans.anchoredPosition3D + animDestPosition;
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

                //now position to the grid
                surrogateCard.gameObject.transform.SetParent(cardHolder.transform);
                var xPos = 195 * (c % rowSize);
                var yPos = c >= rowSize ? -419 : -135;

                surrogateCard.rectTransform.anchorMax = cardAnchor;
                surrogateCard.rectTransform.anchorMin = cardAnchor;
                surrogateCard.rectTransform.pivot = cardAnchor;
                var destPosition = new Vector3(xPos, yPos);
                //surrogateCard.rectTransform.anchoredPosition3D = animStartPosition + destPosition;
                surrogateCard.rectTransform.anchoredPosition3D = destPosition;

                surrogateCard.gameObject.SetActive(true);

                //have to update the text after setting active for some reason to get the text spline to 
                //update correctly
                surrogateCard.cardView.UpdateText(0);
                //iTweenExtensions.MoveToLocal(card.gameObject, destPosition, animTime, 0f);
            }

            //now disable any straggling cards for this page, like if there's only 1 card on the page disable the other 7
            for (int c = cards.Count + createdCardOffset; c < pageSize + createdCardOffset; c++)
            {
                createdCards[c].gameObject.SetActive(false);
            }

            //swip swap the next cards to use by either incrimenting or resetting the created card offset
            createdCardOffset = createdCardOffset == 0 ? pageSize : 0;

            UpdatePageButtons();
        }

        //show/hide paging buttons if there are available pages in that direction
        void UpdatePageButtons()
        {
            if (offset == 0)
            {
                prevButton.gameObject.SetActive(false);
            }
            else
            {
                prevButton.gameObject.SetActive(true);
            }

            if (remainingCardsToShow <= 0)
            {
                nextButton.gameObject.SetActive(false);
            }
            else
            {
                nextButton.gameObject.SetActive(true);
            }
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
            StartCoroutine(ShowRaceSelectionButtons(true));
        }

        void onFactionSelect(Races race)
        {
            StartCoroutine(ShowRaceSelectionButtons(false));
            clickNewDeckSignal.Dispatch(race);
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

        void onDeleteDeckClick()
        {
            clickDeleteDeckSignal.Dispatch(editingDeck);
            ResetRaceFilters();
            editingDeck = null;
            ShowDeckHolder(true);
        }

        internal void onDeckSaved()
        {
            ResetRaceFilters();
            editingDeck = null;
            ShowDeckHolder(true);
        }
        
        internal void onCancelDeck()
        {
            editingDeck = null;
            ShowDeckHolder(true);
            ResetRaceFilters();
        }

        //swap the cards out for faction selection buttons
        //Have to do this in a coroutine for now to prevent the mouse up from selecting the card
        //behind the panel when a new deck is selected. This is because the onClick events for the buttons
        //don't go through the interaction stuff of course
        IEnumerator ShowRaceSelectionButtons(bool show)
        {
            yield return new WaitForSeconds(0.0f);
            cardHolder.SetActive(!show);
            cardControls.SetActive(!show);
            factionSelection.SetActive(show);
        }

        //Show either deck holder, or the deck edit holder
        public void ShowDeckHolder(bool show)
        {
            float showXPos = -74f;
            float hideXPos = 400f;
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
                toggle.Value.isOn = toggle.Key == defaultRace ? true : false;
                toggle.Value.interactable = true;
            }
        }
    }
}


using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ctac
{
    public class CardsMenuMediator : Mediator
    {
        [Inject] public CardsMenuView view { get; set; }

        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public ICardService cardService { get; set; }

        [Inject] public CardDirectory cardDirectory { get; set; }

        [Inject] public CardsKickoffSignal cardKickoff { get; set; }

        [Inject] public NewDeckSignal newDeck { get; set; }
        [Inject] public EditDeckSignal editDeck { get; set; }
        [Inject] public SaveDeckSignal saveDeck { get; set; }
        [Inject] public RemoveDeckSignal removeDeck { get; set; }
        [Inject] public CancelDeckSignal cancelDeck { get; set; }

        public override void OnRegister()
        {
            view.clickLeaveSignal.AddListener(onLeaveClicked);

            view.clickNewDeckSignal.AddListener(onNewDeck);
            view.clickSaveDeckSignal.AddListener(onSaveDeck);
            view.clickCancelDeckSignal.AddListener(onCancelDeck);

            editDeck.AddListener(onEditDeck);

            cardKickoff.AddListener(onKickoff);

            view.init(cardService, cardDirectory);
        }

        public override void onRemove()
        {
            view.clickLeaveSignal.RemoveListener(onLeaveClicked);
            cardKickoff.RemoveListener(onKickoff);

            view.clickNewDeckSignal.RemoveListener(onNewDeck);
            view.clickSaveDeckSignal.RemoveListener(onSaveDeck);
            view.clickCancelDeckSignal.RemoveListener(onCancelDeck);

            editDeck.RemoveListener(onEditDeck);
        }

        public void Update()
        {
        }

        private void onKickoff()
        {
            view.UpdateCards();
        }

        static Dictionary<Races, string> starterDeckNames = new Dictionary<Races, string>()
        {
            {Races.Venusians, "Victory by Patience" },
            {Races.Earthlings, "Easy PZ" },
            {Races.Martians, "March to Victory" },
            {Races.Grex, "Gratuitous Explosions" },
            {Races.Phaenon, "Phenom Phaenon" },
            {Races.Lost, "Laconic Style" },
        };
 
        private void onNewDeck(Races raceSelected)
        {
            var dm = new DeckModel()
            {
                name = starterDeckNames[raceSelected],
                race = raceSelected
            };
            newDeck.Dispatch(dm);
            editDeck.Dispatch(dm);
        }

        private void onEditDeck(DeckModel deck)
        {
            view.onEditDeck(deck);
        }

        private void onSaveDeck(DeckModel deck)
        {
            view.ResetRaceFilters();
            saveDeck.Dispatch(deck);
        }

        private void onCancelDeck()
        {
            view.ResetRaceFilters();
            cancelDeck.Dispatch();
        }

        private void onLeaveClicked()
        {
            StartCoroutine("LoadLevel", "main");
        }

        public IEnumerator LoadLevel(string level)
        {
            SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);

            yield return null;
        }
    }
}


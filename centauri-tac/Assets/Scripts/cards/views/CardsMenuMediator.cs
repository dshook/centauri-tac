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

        [Inject] public NewDeckSignal newDeck { get; set; }
        [Inject] public SelectDeckSignal selectDeck { get; set; }
        [Inject] public SavingDeckSignal savingDeck { get; set; }
        [Inject] public CancelDeckSignal cancelDeck { get; set; }
        [Inject] public DeleteDeckSignal deleteDeck { get; set; }
        [Inject] public ClearDecksSignal clearDecks { get; set; }

        [Inject] public GetDecksSignal getDecks { get; set; }
        [Inject] public SaveDeckSignal saveDeck { get; set; }

        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public LobbyModel lobbyModel { get; set; }
        [Inject] public SwitchLobbyViewSignal moveLobbyView { get; set; }

        public override void OnRegister()
        {
            view.clickNewDeckSignal.AddListener(onNewDeck);
            view.clickSaveDeckSignal.AddListener(onSaveDeck);
            view.clickDeleteDeckSignal.AddListener(onDeleteDeck);

            view.leaveButton.onClick.AddListener(onLeaveClicked);
            view.cancelDeckButton.onClick.AddListener(() => cancelDeck.Dispatch());

            view.init(cardService, cardDirectory);
        }

        public override void OnRemove()
        {
            view.clickNewDeckSignal.RemoveListener(onNewDeck);
            view.clickSaveDeckSignal.RemoveListener(onSaveDeck);
            view.clickDeleteDeckSignal.RemoveListener(onDeleteDeck);
        }


        [ListensTo(typeof(CardsKickoffSignal))]
        public void onKickoff()
        {
            view.UpdateCards();
        }

        [ListensTo(typeof(LobbyLoggedInSignal))]
        public void onLobbyLoggedIn(LoginStatusModel loginStatus, SocketKey key)
        {
            lobbyModel.lobbyKey = key;
            getDecks.Dispatch();
        }

        [ListensTo(typeof(GotDecksSignal))]
        public void onGotDecks(ServerDecksModel decks, SocketKey key)
        {
            debug.Log("Got Decks: " + decks.decks.Count);
            clearDecks.Dispatch();
            foreach (var deck in decks.decks)
            {
                newDeck.Dispatch(deck);
            }
        }

        static Dictionary<Races, string> starterDeckNames = new Dictionary<Races, string>()
        {
            {Races.Vae, "Victory by Patience" },
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
            selectDeck.Dispatch(dm);
        }

        [ListensTo(typeof(SelectDeckSignal))]
        private void onSelectDeck(DeckModel deck)
        {
            if (deck != null)
            {
                view.onEditDeck(deck);
            }
            else
            {
                view.onCancelDeck();
            }
        }

        private void onSaveDeck(DeckModel deck)
        {
            savingDeck.Dispatch(deck);
            saveDeck.Dispatch(deck);
        }

        private void onDeleteDeck(DeckModel deck)
        {
            deleteDeck.Dispatch(deck);
        }

        [ListensTo(typeof(DeckSavedSignal))]
        public void onDeckSaved(DeckModel deck, SocketKey key)
        {
            view.onDeckSaved();
            selectDeck.Dispatch(null);
        }

        [ListensTo(typeof(DeckSaveFailedSignal))]
        public void onDeckSaveFailed(string message, SocketKey key)
        {
            debug.Log("Deck Save failed: " + message, key);
        }

        [ListensTo(typeof(CancelDeckSignal))]
        public void onCancelDeck()
        {
            view.onCancelDeck();
            selectDeck.Dispatch(null);
        }

        private void onLeaveClicked()
        {
            moveLobbyView.Dispatch(LobbyScreens.main);
        }

        public IEnumerator LoadLevel(string level)
        {
            SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);

            yield return null;
        }

    }
}


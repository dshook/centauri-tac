using ctac.signals;
using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ctac
{
    public class ExchangeMenuMediator : Mediator
    {
        [Inject] public ExchangeMenuView view { get; set; }

        // [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public SwitchLobbyViewSignal moveLobbyView { get; set; }
        [Inject] public LobbyModel lobbyModel { get; set; }


        public override void OnRegister()
        {
            view.init();

            view.leaveButton.onClick.AddListener(onLeaveClicked);
        }

        [ListensTo(typeof(MainMenuStartSignal))]
        private void onMainMenuStart()
        {
            //Reset things when the main menu is loaded, such as coming back from a game.
            debug.Log("Resetting exchange menu");
        }

        private void onLeaveClicked()
        {
            moveLobbyView.Dispatch(LobbyScreens.main);
        }

    }
}


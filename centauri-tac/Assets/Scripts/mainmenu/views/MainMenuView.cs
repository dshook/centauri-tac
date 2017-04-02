using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;
using ctac.util;
using UnityEngine;
using SVGImporter;

namespace ctac
{
    public class MainMenuView : View
    {
        public Signal clickPlaySignal = new Signal();
        public Signal clickCardsSignal = new Signal();
        public Signal clickOptionsSignal = new Signal();
        public Signal clickLeaveSignal = new Signal();

        public Button playButton;
        public Button cardsButton;
        public Button optionsButton;
        public Button leaveButton;

        internal void init()
        {
            playButton.onClick.AddListener(() => clickPlaySignal.Dispatch());
            cardsButton.onClick.AddListener(() => clickCardsSignal.Dispatch());
            optionsButton.onClick.AddListener(() => clickOptionsSignal.Dispatch());
            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
        }

        void Update()
        {
        }
    }
}


using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;
using ctac.util;
using UnityEngine;
using SVGImporter;

namespace ctac
{
    public class EndTurnView : View
    {
        public Signal clickEndTurnSignal = new Signal();
        public Signal clickSwitchSidesSignal = new Signal();
        public Signal clickPauseSignal = new Signal();
        public Signal clickResumeSignal = new Signal();

        public Button endTurnButton;
        public Button switchSidesButton;
        public Button pauseButton;
        public Button resumeButton;

        TextMeshProUGUI buttonText;
        SVGImage buttonBg;

        Color bgDoneColor = ColorExtensions.HexToColor("#007422");
        Color textDoneColor = ColorExtensions.HexToColor("#08EA00");

        internal void init()
        {
            endTurnButton.onClick.AddListener(() => clickEndTurnSignal.Dispatch());
            switchSidesButton.onClick.AddListener(() => clickSwitchSidesSignal.Dispatch());
            pauseButton.onClick.AddListener(() => clickPauseSignal.Dispatch());
            resumeButton.onClick.AddListener(() => clickResumeSignal.Dispatch());
            buttonText = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonBg = endTurnButton.GetComponent<SVGImage>();
        }

        void Update()
        {
        }

        internal void onTurnEnded(string text)
        {
            buttonText.text = text;
        }

        internal void updatePlayable(bool anythingToPlay)
        {
            if (anythingToPlay)
            {
                buttonBg.color = Color.white;
                buttonText.color = Color.white;
            }
            else
            {
                buttonBg.color = bgDoneColor;
                buttonText.color = textDoneColor;
            }
        }
    }
}


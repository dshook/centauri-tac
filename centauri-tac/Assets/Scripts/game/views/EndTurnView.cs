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

        public Button endTurnButton;
        public Button switchSidesButton;

        TextMeshProUGUI buttonText;
        SVGImage buttonBg;

        Color bgDoneColor = ColorExtensions.HexToColor("#007422");
        Color textDoneColor = ColorExtensions.HexToColor("#08EA00");

        internal void init()
        {
            endTurnButton.onClick.AddListener(() => onClick());
            switchSidesButton.onClick.AddListener(() => onSwitchSidesClick());
            buttonText = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonBg = endTurnButton.GetComponent<SVGImage>();
        }

        void Update()
        {
        }

        void onClick()
        {
            clickEndTurnSignal.Dispatch();
        }

        void onSwitchSidesClick()
        {
            clickSwitchSidesSignal.Dispatch();
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


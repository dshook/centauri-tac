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
        public Signal clickSignal = new Signal();

        public Button endTurnButton;

        TextMeshProUGUI buttonText;
        SVGImage buttonBg;

        Color bgDoneColor = ColorExtensions.HexToColor("#007422");
        Color textDoneColor = ColorExtensions.HexToColor("#08EA00");

        internal void init()
        {
            endTurnButton.onClick.AddListener(() => onClick());
            buttonText = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonBg = endTurnButton.GetComponent<SVGImage>();
        }

        void Update()
        {
        }

        void onClick()
        {
            clickSignal.Dispatch();
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


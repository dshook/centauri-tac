using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ctac
{
    public class EndTurnView : View
    {
        public Signal clickSignal = new Signal();

        public Button endTurnButton;

        Text buttonText;

        internal void init()
        {
            endTurnButton.onClick.AddListener(() => onClick());
            buttonText = endTurnButton.GetComponentInChildren<Text>();
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
    }
}


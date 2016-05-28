using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;

namespace ctac
{
    public class EndTurnView : View
    {
        public Signal clickSignal = new Signal();

        public Button endTurnButton;

        TextMeshProUGUI buttonText;

        internal void init()
        {
            endTurnButton.onClick.AddListener(() => onClick());
            buttonText = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
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


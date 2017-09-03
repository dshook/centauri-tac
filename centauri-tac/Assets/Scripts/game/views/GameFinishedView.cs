using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;

namespace ctac
{
    public class GameFinishedView : View
    {
        public Signal clickSignal = new Signal();

        public Button mainMenuButton;
        public TextMeshProUGUI finishMessage;

        internal void init()
        {
            mainMenuButton.onClick.AddListener(() => clickSignal.Dispatch());
        }

        internal void onFinish(string message)
        {
            gameObject.SetActive(true);
            finishMessage.text = message;
        }

    }
}


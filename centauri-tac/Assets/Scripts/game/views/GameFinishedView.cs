using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using TMPro;
using UnityEngine;

namespace ctac
{
    public class GameFinishedView : View
    {
        public Signal clickSignal = new Signal();

        public GameObject holder;
        public Button mainMenuButton;
        public TextMeshProUGUI finishMessage;

        internal void init()
        {
            mainMenuButton.onClick.AddListener(() => clickSignal.Dispatch());
        }

        internal void onFinish(string message)
        {
            holder.SetActive(true);
            finishMessage.text = message;
        }

    }
}


using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ctac
{
    public class LeaveGameView : View
    {
        public Signal clickSignal = new Signal();

        public Button endTurnButton;

        internal void init()
        {
            endTurnButton.onClick.AddListener(() => onClick());
        }

        void onClick()
        {
            clickSignal.Dispatch();
        }
    }
}


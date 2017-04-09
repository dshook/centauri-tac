using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ctac
{
    public class LeaveGameView : View
    {
        public Signal<bool> clickSignal = new Signal<bool>();

        public Button endTurnButton;
        public bool returnToMainMenu;

        internal void init()
        {
            endTurnButton.onClick.AddListener(() => onClick());
        }

        void onClick()
        {
            clickSignal.Dispatch(returnToMainMenu);
        }
    }
}


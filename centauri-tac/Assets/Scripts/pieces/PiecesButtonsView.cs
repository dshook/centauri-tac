using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ctac
{
    public class PiecesButtonsView : View
    {
        public Signal clickSignal = new Signal();

        public Button button;

        internal void init()
        {
            button.onClick.AddListener(() => onClick());
        }

        void onClick()
        {
            clickSignal.Dispatch();
        }
    }
}


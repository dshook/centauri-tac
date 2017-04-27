using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ctac
{
    public class CardsMenuView : View
    {
        public Signal clickLeaveSignal = new Signal();

        public Button leaveButton;

        internal void init()
        {
            leaveButton.onClick.AddListener(() => clickLeaveSignal.Dispatch());
        }

        void Update()
        {
        }

    }
}


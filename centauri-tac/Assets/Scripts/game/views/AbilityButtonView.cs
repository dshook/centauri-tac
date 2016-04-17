using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine.EventSystems;

namespace ctac
{
    public class AbilityButtonView : View
    {
        public Signal clickSignal = new Signal();
        public Signal hoverSignal = new Signal();

        public AbilityTarget ability;
        public Button abilityButton;

        internal void init()
        {
            abilityButton.onClick.AddListener(() => onClick());
        }

        void onClick()
        {
            clickSignal.Dispatch();
        }

        void OnPointerEnter(PointerEventData e)
        {
            hoverSignal.Dispatch();
        }
    }
}


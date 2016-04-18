using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine.EventSystems;

namespace ctac
{
    public class AbilityButtonView : View, IPointerEnterHandler, IPointerExitHandler
    {
        public Signal clickSignal = new Signal();
        public Signal<bool> hoverSignal = new Signal<bool>();

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

        public void OnPointerEnter(PointerEventData e)
        {
            hoverSignal.Dispatch(true);
        }

        public void OnPointerExit(PointerEventData e)
        {
            hoverSignal.Dispatch(false);
        }
    }
}


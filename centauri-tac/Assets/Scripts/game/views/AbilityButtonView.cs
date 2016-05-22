using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine.EventSystems;
using TMPro;

namespace ctac
{
    public class AbilityButtonView : View, IPointerEnterHandler, IPointerExitHandler
    {
        public Signal clickSignal = new Signal();
        public Signal<bool> hoverSignal = new Signal<bool>();

        public AbilityTarget ability;
        public Button abilityButton;
        public PieceModel piece;

        private TextMeshProUGUI buttonText;
        private PlayerResourcesModel playerResources;

        internal void init(PlayerResourcesModel playerResources, PieceModel piece)
        {
            abilityButton.onClick.AddListener(() => onClick());
            abilityButton.interactable = false;
            buttonText = abilityButton.GetComponentInChildren<TextMeshProUGUI>();
            this.playerResources = playerResources;
            this.piece = piece;
        }

        void Update()
        {
            buttonText.text = string.Format("({0}) {1}", ability.abilityCost, ability.ability);

            abilityButton.interactable = (ability.abilityCooldown == 0) 
                && ability.abilityCost <= playerResources.resources[piece.playerId];
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


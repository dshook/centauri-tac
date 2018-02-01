using UnityEngine.UI;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine.EventSystems;
using TMPro;

namespace ctac
{
    public class AbilityButtonView : View, IPointerEnterHandler, IPointerExitHandler
    {
        [Inject] public GameInputStatusModel gameInputStatus { get; set; }

        public Signal clickSignal = new Signal();
        public Signal<bool> hoverSignal = new Signal<bool>();

        public AbilityTarget ability;
        public Button abilityButton;
        public PieceModel piece;

        private TextMeshProUGUI buttonText;
        private PlayerResourcesModel playerResources;
        private GamePlayersModel players { get; set; }

        internal void init(PlayerResourcesModel playerResources, PieceModel piece, GamePlayersModel players)
        {
            abilityButton.onClick.AddListener(() => onClick());
            abilityButton.interactable = false;
            buttonText = abilityButton.GetComponentInChildren<TextMeshProUGUI>();
            this.playerResources = playerResources;
            this.piece = piece;
            this.players = players;
        }

        void Update()
        {
            if (abilityButton == null || buttonText == null) return;

            buttonText.text = string.Format("({0}) {1}", ability.abilityCost, ability.ability);

            abilityButton.interactable =
                gameInputStatus.inputEnabled
                && ability.abilityCooldown == 0
                && ability.abilityCost <= playerResources.resources[players.Me.id];
        }

        void onClick()
        {
            if (!gameInputStatus.inputEnabled) return;

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


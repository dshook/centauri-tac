using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class AbilityButtonMediator : Mediator
    {
        [Inject]
        public AbilityButtonView view { get; set; }

        [Inject]
        public ActivateAbilitySignal activateAbility { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onAbilityClicked);
            view.init();
        }

        public override void onRemove()
        {
            view.clickSignal.RemoveListener(onAbilityClicked);
        }

        private void onAbilityClicked()
        {
            activateAbility.Dispatch(new ActivateAbilityModel() {
                piece = pieces.Piece(view.ability.pieceId),
                optionalTarget = null
            });
        }

    }
}


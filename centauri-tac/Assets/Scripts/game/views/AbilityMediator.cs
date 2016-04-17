using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class AbilityMediator : Mediator
    {
        [Inject]
        public AbilityView view { get; set; }

        [Inject]
        public PossibleActionsReceivedSignal possibleActions { get; set; }

        public override void OnRegister()
        {
            possibleActions.AddListener(onActions);
            view.init();
        }

        public override void onRemove()
        {
            possibleActions.RemoveListener(onActions);
        }

        private void onActions(PossibleActions possible)
        {
            view.UpdateAbilities(possible.playerId, possible.abilities);
        }

    }
}


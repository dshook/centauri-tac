using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class AbilityMediator : Mediator
    {
        [Inject]
        public AbilityView view { get; set; }

        [Inject] public PossibleActionsReceivedSignal possibleActionsSignal { get; set; }
        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        public override void OnRegister()
        {
            possibleActionsSignal.AddListener(onActions);
            turnEnded.AddListener(onTurnEnded);
            view.init();
        }

        public override void onRemove()
        {
            possibleActionsSignal.RemoveListener(onActions);
            turnEnded.RemoveListener(onTurnEnded);
        }

        private void onActions(PossibleActions possible)
        {
            view.UpdateAbilities(possible.playerId, possible.abilities);
        }

        private void onTurnEnded(GameTurnModel turnModel)
        {
            if (players.isHotseat && possibleActions.possibleAbilities.ContainsKey(players.Me.id))
            {
                view.UpdateAbilities(players.Me.id, possibleActions.possibleAbilities[players.Me.id]);
            }
        }

    }
}


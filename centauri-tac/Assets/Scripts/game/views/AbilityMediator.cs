using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class AbilityMediator : Mediator
    {
        [Inject] public AbilityView view { get; set; }

        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        public override void OnRegister()
        {
            view.init();
        }

        [ListensTo(typeof(PossibleActionsReceivedSignal))]
        public void onActions(PossibleActions possible)
        {
            view.UpdateAbilities(possible.playerId, possible.abilities);
        }

        [ListensTo(typeof(TurnEndedSignal))]
        public void onTurnEnded(GameTurnModel turnModel)
        {
            if (players.isHotseat && possibleActions.possibleAbilities.ContainsKey(players.Me.id))
            {
                view.UpdateAbilities(players.Me.id, possibleActions.possibleAbilities[players.Me.id]);
            }
        }

    }
}


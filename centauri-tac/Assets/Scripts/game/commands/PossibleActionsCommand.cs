using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class PossibleActionsCommand : Command
    {
        [Inject]
        public PossibleActions newPossibleActions { get; set; }

        [Inject]
        public PossibleActionsModel possibleActions { get; set; }

        [Inject]
        public PossibleActionsReceivedSignal possibleActionsReceived { get; set; }

        public override void Execute()
        {
            possibleActions.Update(newPossibleActions);

            possibleActionsReceived.Dispatch(newPossibleActions);
        }
    }
}


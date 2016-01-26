using strange.extensions.command.impl;

namespace ctac
{
    public class PossibleActionsCommand : Command
    {
        [Inject]
        public PossibleActions newPossibleActions { get; set; }

        [Inject]
        public PossibleActionsModel possibleActions { get; set; }

        public override void Execute()
        {
            possibleActions.Update(newPossibleActions);
        }
    }
}


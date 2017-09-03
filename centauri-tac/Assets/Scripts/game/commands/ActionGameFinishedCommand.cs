using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class ActionGameFinishedCommand : Command
    {
        [Inject] public ISocketService socket { get; set; }
        [Inject] public IDebugService debug { get; set; }

        [Inject] public GameFinishedModel gameFinished { get; set; }
        [Inject] public SocketKey socketKey { get; set; }

        [Inject] public GameTurnModel turns { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        [Inject] public ActionMessageSignal message { get; set; }
        [Inject] public GameFinishedSignal gameFinishedSignal { get; set; }

        [Inject] public ActionsProcessedModel processedActions { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(gameFinished.id)) return;

            gameFinishedSignal.Dispatch(gameFinished);
        }
    }
}


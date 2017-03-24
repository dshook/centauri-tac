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

            if (gameFinished.winnerId == players.Me.id)
            {
                message.Dispatch(new MessageModel() { message = "Victory!", duration = 5000}, socketKey);
            }
            else
            {
                message.Dispatch(new MessageModel() { message = "Defeat :(", duration = 5000}, socketKey);
            }
            gameFinishedSignal.Dispatch(gameFinished);
        }
    }
}


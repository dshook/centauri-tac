using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class GameFinishedCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public GameFinishedModel gameFinished { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public GameTurnModel turns { get; set; }

        [Inject]
        public ActionMessageSignal message { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(gameFinished.id)) return;

            if (gameFinished.winnerId == turns.currentPlayerId)
            {
                message.Dispatch(new MessageModel() { message = "Victory!"}, socketKey);
            }
            else
            {
                message.Dispatch(new MessageModel() { message = "Defeat :("}, socketKey);
            }
        }
    }
}


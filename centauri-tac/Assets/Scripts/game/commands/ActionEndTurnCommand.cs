using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionEndTurnCommand : Command
    {

        [Inject]
        public EndTurnSignal endTurn { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        [Inject]
        public MinionsModel minionsModel { get; set; }

        [Inject]
        public GameTurnModel turnModel { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public GamePassTurnModel gamePassModel { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            //check to see if this action has already been processed by another player
            if (processedActions.processedActions.Any(x => x == gamePassModel.id))
            {
                return;
            }
            processedActions.processedActions.Add(gamePassModel.id);

            turnModel.currentTurn = gamePassModel.id;
            turnModel.currentTurnClientId = gamePlayers.players.First(x => x.id == gamePassModel.to).clientId;
            foreach (var minion in minionsModel.minions)
            {
                minion.hasMoved = false;
                if (minion.playerId == gamePassModel.to)
                {
                    minion.currentPlayerHasControl = true;
                }
                else
                {
                    minion.currentPlayerHasControl = false;
                }
            }
            debug.Log("Turn Ended");
            turnEnded.Dispatch();
        }
    }
}


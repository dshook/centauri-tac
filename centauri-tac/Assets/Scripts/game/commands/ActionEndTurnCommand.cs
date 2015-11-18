using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionEndTurnCommand : Command
    {
        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public GameTurnModel turnModel { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public PassTurnModel gamePassModel { get; set; }

        [Inject]
        public PlayerResourcesModel playerResources { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

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

            turnModel.currentTurn = gamePassModel.currentTurn;
            turnModel.currentPlayerId = gamePassModel.to;
            turnModel.currentTurnClientId = gamePlayers.players.First(x => x.id == gamePassModel.to).clientId;
            playerResources.resources[gamePassModel.to] = gamePassModel.toPlayerResources;
            foreach (var piece in piecesModel.Pieces)
            {
                piece.hasMoved = false;
                piece.hasAttacked = false;
                if (piece.playerId == gamePassModel.to)
                {
                    piece.currentPlayerHasControl = true;
                }
                else
                {
                    piece.currentPlayerHasControl = false;
                }
            }
            debug.Log("Turn Ended");
            turnEnded.Dispatch();
        }
    }
}


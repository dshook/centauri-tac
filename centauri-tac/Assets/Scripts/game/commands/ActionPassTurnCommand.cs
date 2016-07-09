using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPassTurnCommand : Command
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
            if (!processedActions.Verify(gamePassModel.id)) return;

            turnModel.currentTurn = gamePassModel.currentTurn;
            turnModel.currentPlayerId = gamePassModel.to;
            turnModel.currentTurnClientId = gamePlayers.GetByPlayerId(gamePassModel.to).clientId;
            playerResources.resources[gamePassModel.to] = gamePassModel.toPlayerResources;
            playerResources.maxResources[gamePassModel.to] = gamePassModel.toPlayerMaxResources;

            var opponentId = gamePlayers.OpponentId(turnModel.currentPlayerId);
            foreach (var piece in piecesModel.Pieces)
            {
                piece.hasMoved = false;
                piece.attackCount = 0;
                if (piece.playerId == opponentId)
                {
                    piece.currentPlayerHasControl = false;
                }
                else
                {
                    piece.currentPlayerHasControl = true;
                }
            }
            debug.Log("Turn Ended");
            turnEnded.Dispatch(turnModel);
        }
    }
}


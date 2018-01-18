using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;

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

            foreach (var resourceModel in gamePassModel.playerResources)
            {
                playerResources.resources[resourceModel.playerId] = resourceModel.current;
                playerResources.maxResources[resourceModel.playerId] = resourceModel.max;
            }

            foreach (var piece in piecesModel.Pieces)
            {
                piece.moveCount = 0;
                piece.attackCount = 0;
                piece.age++;

                //stop the z's
                var sleeping = piece.pieceView.faceCameraContainer.transform.Find("Sleeping Status");
                if(sleeping != null){
                    var particle = sleeping.GetComponent<ParticleSystem>();
                    if(particle != null){
                        particle.Stop();
                    }
                    GameObject.Destroy(particle.gameObject, 3f);
                }
            }

            debug.Log("Turn Ended");
            turnEnded.Dispatch(turnModel);
        }
    }
}


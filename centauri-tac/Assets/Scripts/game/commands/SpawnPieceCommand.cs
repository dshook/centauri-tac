using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;
using System.Linq;
using System;
using strange.extensions.context.api;

namespace ctac
{
    public class SpawnPieceCommand : Command
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public SpawnPieceModel spawnedPiece { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public MinionsModel minionsModel { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public GameTurnModel turnModel { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions
        {
            get; set;
        }

            private GameObject _minionPrefab { get; set; }
        private GameObject minionPrefab
        {
            get
            {
                if (_minionPrefab == null)
                {
                    _minionPrefab = Resources.Load("Minion") as GameObject;
                }
                return _minionPrefab;
            }
        }

        public override void Execute()
        {
            //check to see if this action has already been processed by another player
            if (processedActions.processedActions.Any(x => x == spawnedPiece.id))
            {
                return;
            }
            processedActions.processedActions.Add(spawnedPiece.id);


            var newMinion = GameObject.Instantiate(
                minionPrefab, 
                spawnedPiece.position.Vector3,
                Quaternion.identity
            ) as GameObject;
            newMinion.transform.parent = contextView.transform;

            //set up display
            try
            {
                //TODO: caching for loading resources might be a good idea
                //var idleAnimation = Resources.Load("Minions/" + spawnedPiece.pieceResourceId + "/Idle") as Animation;
                var animationController = Resources.Load("Minions/" + spawnedPiece.pieceResourceId + "/Unit") as RuntimeAnimatorController;

                var animator = newMinion.GetComponentInChildren<Animator>();
                animator.runtimeAnimatorController = animationController;

            }
            catch (Exception ex)
            {
                debug.LogError("Could not load resources for id " + spawnedPiece.pieceResourceId + " " + ex.ToString(), socketKey);
            }

            var currentPlayerId = gamePlayers.players.First(x => x.clientId == turnModel.currentTurnClientId).id;

            var minionModel = new MinionModel()
            {
                id = spawnedPiece.pieceId,
                playerId = spawnedPiece.playerId,
                currentPlayerHasControl = spawnedPiece.playerId == currentPlayerId,
                gameObject = newMinion,
                attack = spawnedPiece.attack,
                health = spawnedPiece.health,
                originalAttack = spawnedPiece.attack,
                originalHealth = spawnedPiece.health,
                moveDist = 5
            };

            newMinion.AddComponent<MinionView>();
            newMinion.GetComponent<MinionView>().minion = minionModel;

            minionsModel.minions.Add(minionModel);

            debug.Log(string.Format("Spawned minion {0} for player {1}", spawnedPiece.pieceResourceId, spawnedPiece.playerId), socketKey);
        }
    }
}


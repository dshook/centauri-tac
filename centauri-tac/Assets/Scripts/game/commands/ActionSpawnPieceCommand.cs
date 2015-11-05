using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;
using System.Linq;
using System;
using strange.extensions.context.api;
using System.Collections.Generic;

namespace ctac
{
    public class ActionSpawnPieceCommand : Command
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public SpawnPieceModel spawnedPiece { get; set; }

        [Inject]
        public PieceSpawnedSignal pieceSpawned { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public GameTurnModel turnModel { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public IResourceLoaderService resourceLoader { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }
        
        [Inject]
        public AnimationQueueModel animations { get; set; }

        private GameObject _piecePrefab { get; set; }
        private GameObject piecePrefab
        {
            get
            {
                if (_piecePrefab == null)
                {
                    _piecePrefab = Resources.Load("Piece") as GameObject;
                }
                return _piecePrefab;
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


            var newPiece = GameObject.Instantiate(
                piecePrefab, 
                spawnedPiece.position.Vector3,
                Quaternion.identity
            ) as GameObject;
            newPiece.transform.parent = contextView.transform;

            //set up display
            try
            {
                var animationController = resourceLoader.LoadPieceRAC(spawnedPiece.pieceResourceId);

                var animator = newPiece.GetComponentInChildren<Animator>();
                animator.runtimeAnimatorController = animationController;
                animator.Play("Idle");

            }
            catch (Exception ex)
            {
                debug.LogError("Could not load resources for id " + spawnedPiece.pieceResourceId + " " + ex.ToString(), socketKey);
            }

            var currentPlayerId = gamePlayers.players.First(x => x.clientId == turnModel.currentTurnClientId).id;
            var cardTemplate = cardDirectory.directory.FirstOrDefault(c => c.id == spawnedPiece.pieceResourceId);

            var pieceModel = new PieceModel()
            {
                id = spawnedPiece.pieceId,
                playerId = spawnedPiece.playerId,
                currentPlayerHasControl = spawnedPiece.playerId == currentPlayerId,
                gameObject = newPiece,
                attack = cardTemplate.attack,
                health = cardTemplate.health,
                originalAttack = cardTemplate.attack,
                originalHealth = cardTemplate.health,
                movement = cardTemplate.movement
            };

            var pieceView = newPiece.AddComponent<PieceView>();
            pieceView.piece = pieceModel;

            piecesModel.Pieces.Add(pieceModel);

            pieceSpawned.Dispatch(pieceModel);
            debug.Log(string.Format("Spawned piece {0} for player {1}", spawnedPiece.pieceResourceId, spawnedPiece.playerId), socketKey);
        }
    }
}


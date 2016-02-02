using UnityEngine;
using System.Collections.Generic;
using System;
using strange.extensions.context.api;
using System.Linq;

namespace ctac
{
    public interface IPieceService
    {
        PieceModel CreatePiece(SpawnPieceModel spawnedPiece, string name = null);
    }

    public class PieceService : IPieceService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public GameTurnModel turnModel { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public IResourceLoaderService resourceLoader { get; set; }

        [Inject]
        public MapModel map { get; set; }

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

        public PieceModel CreatePiece(SpawnPieceModel spawnedPiece, string name = null)
        {
            //position is x and z from server, and y based on the map
            var spawnPosition = map.tiles[spawnedPiece.position.Vector2].fullPosition;

            var newPiece = GameObject.Instantiate(
                piecePrefab, 
                spawnPosition,
                Quaternion.identity
            ) as GameObject;
            newPiece.transform.parent = contextView.transform;

            //set up display
            try
            {
                var animationController = resourceLoader.LoadPieceRAC(spawnedPiece.cardTemplateId);

                var animator = newPiece.GetComponentInChildren<Animator>();
                animator.runtimeAnimatorController = animationController;
                animator.Play("Idle");

            }
            catch (Exception ex)
            {
                debug.LogError("Could not load resources for id " + spawnedPiece.cardTemplateId + " " + ex.ToString());
            }

            var currentPlayerId = gamePlayers.players.First(x => x.clientId == turnModel.currentTurnClientId).id;
            var cardTemplate = cardDirectory.Card(spawnedPiece.cardTemplateId);

            var pieceModel = new PieceModel()
            {
                id = spawnedPiece.pieceId,
                playerId = spawnedPiece.playerId,
                cardTemplateId = spawnedPiece.cardTemplateId,
                currentPlayerHasControl = spawnedPiece.playerId == currentPlayerId,
                gameObject = newPiece,
                attack = cardTemplate.attack,
                health = cardTemplate.health,
                baseAttack = cardTemplate.attack,
                baseHealth = cardTemplate.health,
                movement = cardTemplate.movement,
                tags = spawnedPiece.tags,
                buffs = new List<PieceBuffModel>(),
                hasAttacked = true,
                hasMoved = true
            };

            var pieceView = newPiece.AddComponent<PieceView>();
            pieceView.piece = pieceModel;

            piecesModel.Pieces.Add(pieceModel);

            return pieceModel;
        }
    }
}


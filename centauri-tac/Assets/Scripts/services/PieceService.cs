using UnityEngine;
using System.Collections.Generic;

namespace ctac
{
    public interface IPieceService
    {
        PieceModel CreatePiece(SpawnPieceModel spawnedPiece, string name = null);
        void CopyPropertiesFromPiece(PieceModel src, PieceModel dest);
        void SetInitialMoveAttackStatus(PieceModel piece);
        void CopyPieceToCard(PieceModel src, CardModel dest, bool link);
    }

    public class MockPieceService : IPieceService
    {
        public PieceModel CreatePiece(SpawnPieceModel spawnedPiece, string name = null) { return new PieceModel(); }
        public void CopyPropertiesFromPiece(PieceModel src, PieceModel dest) { }
        public void SetInitialMoveAttackStatus(PieceModel piece) { }
        public void CopyPieceToCard(PieceModel src, CardModel dest, bool link) { }
    }

    public class PieceService : IPieceService
    {
        [Inject(InjectionKeys.GameSignalsRoot)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public IResourceLoaderService loader { get; set; }

        //[Inject]
        //public ICrossContextInjectionBinder injectionBinder { get; set; }

        [Inject]
        public MapModel map { get; set; }

        public PieceModel CreatePiece(SpawnPieceModel spawnedPiece, string name = null)
        {
            GameObject pieceModelResource = loader.Load<GameObject>("Models/" + spawnedPiece.cardTemplateId + "/prefab");

            var piecePrefab = loader.Load<GameObject>("Piece");

            //position is x and z from server, and y based on the map, but way up in the air so the animation can do its job right
            var spawnPosition = map.tiles[spawnedPiece.position.Vector2].fullPosition + new Vector3(0, 100f, 0);

            var newPiece = GameObject.Instantiate(
                piecePrefab, 
                spawnPosition,
                Quaternion.identity
            ) as GameObject;
            newPiece.transform.parent = contextView.transform;
            newPiece.name = "Piece " + spawnedPiece.pieceId;

            //Set up new model if we have one
            if (pieceModelResource != null)
            {
                var pieceModelChild = newPiece.transform.Find("Model");
                pieceModelChild.DestroyChildren(true);
                var newModelInstance = GameObject.Instantiate(pieceModelResource, pieceModelChild, false) as GameObject;
                newModelInstance.transform.localPosition = Vector3.zero;
                //newModelInstance.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
            }

            //go through all the mesh colliders in the piece and tag them as piece so the raycast can know what its hitting
            var meshColliders = newPiece.transform.GetComponentsInChildren<MeshCollider>();
            foreach (var meshCollider in meshColliders)
            {
                meshCollider.gameObject.tag = "Piece";
            }

            var cardTemplate = cardDirectory.Card(spawnedPiece.cardTemplateId);

            var pieceModel = new PieceModel()
            {
                id = spawnedPiece.pieceId.Value,
                playerId = spawnedPiece.playerId,
                cardTemplateId = spawnedPiece.cardTemplateId,
                currentPlayerHasControl = spawnedPiece.playerId == gamePlayers.Me.id,
                gameObject = newPiece,
                attack = cardTemplate.attack,
                health = cardTemplate.health,
                baseAttack = cardTemplate.attack,
                baseHealth = cardTemplate.health,
                tilePosition = spawnPosition.ToTileCoordinates(),
                direction = spawnedPiece.direction,
                movement = cardTemplate.movement,
                baseMovement = cardTemplate.movement,
                range = cardTemplate.range,
                baseRange = cardTemplate.range,
                tags = spawnedPiece.tags ?? cardTemplate.tags,
                buffs = new List<PieceBuffModel>(),
                statuses = cardTemplate.statuses | (cardTemplate.range.HasValue ? Statuses.isRanged : Statuses.None),
            };

            SetInitialMoveAttackStatus(pieceModel);

            var pieceView = newPiece.AddComponent<PieceView>();
            pieceView.loader = loader;

            pieceView.piece = pieceModel;

            piecesModel.Pieces.Add(pieceModel);

            return pieceModel;
        }

        public void CopyPropertiesFromPiece(PieceModel src, PieceModel dest)
        {
            dest.name = src.name;
            dest.cardTemplateId = src.cardTemplateId;
            dest.attack = src.attack;
            dest.baseAttack = src.baseAttack;
            dest.health = src.health;
            dest.baseHealth = src.baseHealth;
            dest.movement = src.movement;
            dest.baseMovement = src.movement;
            dest.range = src.range;
            dest.baseRange = src.baseRange;
            dest.tags = src.tags;
            dest.statuses = src.statuses | (src.range.HasValue ? Statuses.isRanged : Statuses.None);
            dest.buffs = src.buffs;
        }

        public void CopyPieceToCard(PieceModel src, CardModel dest, bool link)
        {
            var templateCard = cardDirectory.Card(src.cardTemplateId);
            dest.cardTemplateId = src.cardTemplateId;
            dest.playerId = src.playerId;
            dest.name = templateCard.name;
            dest.description = templateCard.description;
            dest.cost = templateCard.cost;
            dest.baseCost = templateCard.baseCost;
            dest.attack = src.attack;
            dest.health = src.health;
            dest.movement = src.movement;
            dest.range = src.range;
            dest.tags = src.tags;
            dest.statuses = src.statuses | (src.range.HasValue ? Statuses.isRanged : Statuses.None);
            dest.metCondition = false;

            if (link)
            {
                dest.linkedPiece = src;
            }
        }

        public void SetInitialMoveAttackStatus(PieceModel piece)
        {
            piece.moveCount = !FlagsHelper.IsSet(piece.statuses, Statuses.Charge) && piece.isMelee ? 9999 : 0;
            piece.attackCount = FlagsHelper.IsSet(piece.statuses, Statuses.Charge) ? 0 : 9;
        }
    }
}


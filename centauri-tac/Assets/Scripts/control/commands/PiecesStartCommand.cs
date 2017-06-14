using UnityEngine;
using strange.extensions.command.impl;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;

namespace ctac
{
    public class PiecesStartCommand : Command
    {
        [Inject] public PiecesModel piecesModel { get; set; }
        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public MapModel map { get; set; }

        [Inject] public IMapCreatorService mapCreator { get; set; }
        [Inject] public IPieceService pieceService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }
        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            cardDirectory.LoadCards();
            debug.Log("Loaded " + cardDirectory.directory.Count + " cards");
            var minionCards = cardDirectory.directory.Where(c => c.isMinion || c.isHero).ToList();
            
            //figure out what kind of map we want
            var defaultMap = new MapImportModel()
            {
                name = "Piece generated map",
                maxPlayers = 2,
                tiles = new List<TileImport>()
            };
            var rows = 5;
            var columns = (int)Math.Ceiling((double)minionCards.Count / rows);
            //space it out by double
            for (int r = 0; r < rows * 2; r++)
            {
                for (int c = 0; c < columns* 2; c++)
                {
                    defaultMap.tiles.Add(new TileImport()
                    {
                        material = "grass",
                        unpassable = false,
                        transform = new TileImportPosition() { x = r, y = 1f, z = c }
                    });
                }
            }

            mapCreator.CreateMap(defaultMap);

            //remove any minions and cards on the board for a clean slate
            piecesModel.Pieces = new List<PieceModel>();
            var taggedPieces = GameObject.FindGameObjectsWithTag("Piece");
            foreach (var piece in taggedPieces)
            {
                GameObject.Destroy(piece);
            }
            var taggedCards = GameObject.FindGameObjectsWithTag("Card");
            foreach (var card in taggedCards)
            {
                GameObject.Destroy(card);
            }

            //setup fake players
            var meId = Guid.NewGuid();
            var opponentId = Guid.NewGuid();
            players.AddOrUpdate(new PlayerModel()
            {
                clientId = meId,
                isLocal = true,
                id = 1,
            });
            players.AddOrUpdate(new PlayerModel()
            {
                clientId = opponentId,
                isLocal = true,
                id = 2
            });
            players.SetMeClient(meId);

            var tileText = loader.Load<GameObject>("TileText");

            //and create pieces finally
            int pRow = 0;
            int pCol = 0;
            //var spawnIds = new List<int>() { 77 };
            for (int p = 0; p < minionCards.Count; p++)
            {
                var minionCard = minionCards[p];

                //if (!spawnIds.Contains(minionCard.cardTemplateId)) { continue; }
                var piecePosition = new Vector2(pRow * 2, pCol * 2);

                pieceService.CreatePiece(new SpawnPieceModel()
                {
                    pieceId = minionCard.cardTemplateId,
                    cardTemplateId = minionCard.cardTemplateId,
                    playerId = 1,
                    position = new PositionModel(piecePosition),
                    direction = Direction.South
                });

                //setup tile text on map
                var tile = map.tiles[piecePosition];
                var newTileText = GameObject.Instantiate(tileText, tile.gameObject.transform, false);
                var tileTMP = newTileText.GetComponent<TextMeshPro>();
                tileTMP.text = minionCard.cardTemplateId.ToString();

                pRow++;
                if (pRow >= rows)
                {
                    pRow = 0;
                    pCol++;
                }
            }
        }
    }
}


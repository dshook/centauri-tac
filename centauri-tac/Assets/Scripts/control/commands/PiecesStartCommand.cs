using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using ctac.signals;
using System;
using System.Linq;

namespace ctac
{
    public class PiecesStartCommand : Command
    {

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }


        [Inject] public PiecesModel piecesModel { get; set; }
        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        [Inject] public IMapCreatorService mapCreator { get; set; }
        [Inject] public IPieceService pieceService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }
        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            //fetch all cards from disk
            int numberOfCards = 0;
            foreach (string file in Directory.GetFiles("../cards", "*.json"))
            {
                string cardText = File.ReadAllText(file);
                var cardTemplate = JsonConvert.DeserializeObject<CardModel>(cardText);
                cardTemplate.baseCost = cardTemplate.cost;
                cardDirectory.directory.Add(cardTemplate);
                numberOfCards++;
            }
            debug.Log("Loaded " + numberOfCards + " cards");
            var minionCards = cardDirectory.directory.Where(c => c.isMinion).ToList();
            
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

            //and create pieces finally
            int pRow = 0;
            int pCol = 0;
            //var spawnIds = new List<int>() { 63, 68, 57, 8, 92 };
            for (int p = 0; p < minionCards.Count; p++)
            {
                var minionCard = minionCards[p];

                //if (!spawnIds.Contains(minionCard.cardTemplateId)) { continue; }

                pieceService.CreatePiece(new SpawnPieceModel()
                {
                    pieceId = minionCard.cardTemplateId,
                    cardTemplateId = minionCard.cardTemplateId,
                    playerId = 1,
                    position = new PositionModel(new Vector2(pRow * 2, pCol * 2)),
                    direction = Direction.South
                });

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


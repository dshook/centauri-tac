using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using ctac.signals;
using System;

namespace ctac
{
    public class PiecesStartCommand : Command
    {

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }


        [Inject] public PiecesModel piecesModel { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public DecksModel decks { get; set; }
        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public GamePlayersModel players { get; set; }

        [Inject] public IMapCreatorService mapCreator { get; set; }
        [Inject] public IPieceService pieceService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }
        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            //fetch map from disk, eventually comes from server
            string mapContents = File.ReadAllText("../maps/cubeland.json");
            var defaultMap = JsonConvert.DeserializeObject<MapImportModel>(mapContents);
            debug.Log("Loaded Map");

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

            mapCreator.CreateMap(defaultMap);

            //remove any minions on the board for a clean slate
            piecesModel.Pieces = new List<PieceModel>();
            var taggedPieces = GameObject.FindGameObjectsWithTag("Piece");
            foreach (var piece in taggedPieces)
            {
                GameObject.Destroy(piece);
            }

            //clean up scene cards, init lists.  Need better place for init
            cards.Cards = new List<CardModel>();
            decks.Cards = new List<CardModel>();
            var taggedCards = GameObject.FindGameObjectsWithTag("Card");
            foreach (var card in taggedCards)
            {
                GameObject.Destroy(card);
            }

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

            pieceService.CreatePiece(new SpawnPieceModel()
            {
                pieceId = 1,
                cardTemplateId = 1,
                playerId = 1,
                position = new PositionModel(new Vector2(1, 1)),
                direction = Direction.South
            });
        }
    }
}


using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public class StartCommand : Command
    {

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public ConfigModel config { get; set; }

        [Inject]
        public FetchComponentsSignal fetchComponents { get; set; }

        [Inject]
        public PiecesModel piecesModel { get; set; }

        [Inject]
        public IMapCreatorService mapCreator { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        public override void Execute()
        {
            //override config from settings on disk if needed
            string configContents = File.ReadAllText("./config.json");
            if (!string.IsNullOrEmpty(configContents)) {
                var diskConfig = JsonConvert.DeserializeObject<ConfigModel>(configContents);
                diskConfig.CopyProperties(config);
            }

            fetchComponents.Dispatch();

            //fetch map from disk, eventually comes from server
            string mapContents = File.ReadAllText("../maps/cubeland.json");
            var defaultMap = JsonConvert.DeserializeObject<MapImportModel>(mapContents);

            mapCreator.CreateMap(defaultMap);

            //remove any minions on the board for a clean slate
            piecesModel.Pieces = new List<PieceModel>();
            var taggedPieces = GameObject.FindGameObjectsWithTag("Piece");
            foreach (var piece in taggedPieces)
            {
                GameObject.Destroy(piece);
            }

            //give each player some cards, also should come from server ofc
            cards.Cards = new List<CardModel>()
            {
                new CardModel() {
                    id = 1,
                    playerId = 1,
                    name = "Dude",
                    description = "I do stuff, sometimes",
                    attack = 1,
                    health = 2
                },
                new CardModel() {
                    id = 2,
                    playerId = 2,
                    name = "Dudette",
                    description = "I always do something",
                    attack = 7,
                    health = 7
                },
            };
        }
    }
}


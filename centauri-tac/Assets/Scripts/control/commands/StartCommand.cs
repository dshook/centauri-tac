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

        [Inject] public ConfigModel config { get; set; }


        [Inject] public TryLoginSignal tryLoginSignal { get; set; }
        [Inject] public ServerAuthSignal serverAuthSignal { get; set; }
        [Inject] public JoinGameSignal joinGame { get; set; }
        [Inject] public CurrentGameModel currentGame { get; set; }

        [Inject] public PiecesModel piecesModel { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public DecksModel decks { get; set; }
        [Inject] public CardDirectory cardDirectory { get; set; }

        [Inject] public IMapCreatorService mapCreator { get; set; }
        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            //Starting up the game not from main menu
            if (currentGame == null || currentGame.game == null)
            {
                //override config from settings on disk if needed
                string configContents = File.ReadAllText("./config.json");
                if (!string.IsNullOrEmpty(configContents))
                {
                    debug.Log("Reading Config File");
                    var diskConfig = JsonConvert.DeserializeObject<ConfigModel>(configContents);
                    diskConfig.CopyProperties(config);
                }

#if DEBUG
                if (config.players.Count > 0)
                {
                    foreach (var player in config.players)
                    {
                        tryLoginSignal.Dispatch(new Credentials() { username = player.username, password = player.password });
                    }
                }
                else
                {
                    serverAuthSignal.Dispatch();
                }
#endif
            }
            else
            {
                //let the server know we're ready
                joinGame.Dispatch(currentGame.me);
            }

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

#if !DEBUG
            var dbgButtons = GameObject.Find("DebugButtons");
            GameObject.DestroyImmediate(dbgButtons);
#endif
        }
    }
}


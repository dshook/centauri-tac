using UnityEngine;
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
        [Inject] public CardsLoadedSignal cardsLoadedSignal { get; set; }
        [Inject] public MatchmakerQueueSignal matchmakerQueue { get; set; }

        [Inject] public GameLoggedInSignal gameLoggedInSignal { get; set; }
        [Inject] public CurrentGameRegisteredSignal currentGameSignal { get; set; }
        [Inject] public LobbyLoggedInSignal lobbyLoggedInSignal { get; set; }

        [Inject] public PiecesModel piecesModel { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public DecksModel decks { get; set; }
        [Inject] public CardDirectory cardDirectory { get; set; }

        [Inject] public IMapCreatorService mapCreator { get; set; }
        [Inject] public IJsonNetworkService network { get; set; }
        [Inject] public IDebugService debug { get; set; }

        bool isStandaloneLaunch = false; //is the game launching by itself with config for both of the players?
        bool loading = false;

        /// <summary>
        /// Bootstrap the game up. This command and its state persists between launches though so make sure to 
        /// clean up and reset between launches
        /// </summary>
        public override void Execute()
        {
            currentGameSignal.AddListener(onCurrentGame);
            gameLoggedInSignal.AddListener(onGameLoggedIn);
            lobbyLoggedInSignal.AddListener(onLobbyLogin);

            Retain();
            Cleanup();

            //Starting up the game not from main menu
            if (currentGame == null || currentGame.game == null)
            {
                isStandaloneLaunch = true;
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
                    debug.LogWarning("Standalone game launch not supported without config");
                    return;
                }
#endif
            }

            LoadGame();
        }

        public void onCurrentGame(CurrentGameModel game)
        {
            Cleanup();
            LoadGame();
        }

        Dictionary<SocketKey, LoginStatusModel> gameLogins = new Dictionary<SocketKey, LoginStatusModel>();

        public void onGameLoggedIn(LoginStatusModel status, SocketKey key)
        {
            if(!gameLogins.ContainsKey(key)){
                gameLogins.Add(key, status);
            }else{
                gameLogins[key] = status;
            }
            TestLoadReady();
        }

        //Call to load the map once the currentGame is sorted out
        void LoadGame()
        {
            if (currentGame == null || currentGame.game == null)
            {
                debug.LogWarning("Trying to start game without current game");
                return;
            }
            if(loading){
                debug.Log("Already Loading"); //Don't double load in dev mode
                return;
            }
            loading = true;

            cardsLoadedSignal.AddListener(cardsFinishedLoading);
            cardDirectory.LoadCards(network, cardsLoadedSignal);

            mapCreator.CreateMap(currentGame.game.mapData);
            debug.Log("Loaded Map " + currentGame.game.map);
        }

        bool cardsLoaded = false;
        private void cardsFinishedLoading()
        {
            debug.Log("Loaded " + cardDirectory.directory.Count + " cards");
            cardsLoaded = true;
            cardsLoadedSignal.RemoveListener(cardsFinishedLoading);
            TestLoadReady();
        }

        //Coordinate between all (dev) players logging into the game and the cards being loaded
        //Only join the game if both have happened to tell the server we're ready to go
        private void TestLoadReady()
        {
            var finished = false;
            if(!cardsLoaded) return;

            if (!isStandaloneLaunch)
            {
                debug.Log("Player loaded, joining game now");
                joinGame.Dispatch(new LoginStatusModel() { status = true } , currentGame.me, currentGame.game);
                finished = true;
            }
            else if(config.players == null || config.players.Count == 0 || gameLogins.Count >= config.players.Count){
                debug.Log("All players loaded, joining game now");
                foreach(var gameLogin in gameLogins){
                    //let the server know we're ready
                    joinGame.Dispatch(new LoginStatusModel() { status = true } , gameLogin.Key, currentGame.game);
                }
                finished = true;
            }

            if (finished)
            {
                loading = false;
                Release();
                currentGameSignal.RemoveListener(onCurrentGame);
                gameLoggedInSignal.RemoveListener(onGameLoggedIn);
                lobbyLoggedInSignal.RemoveListener(onLobbyLogin);
            }
        }

        public void onLobbyLogin(LoginStatusModel lsm, SocketKey key)
        {
            if (isStandaloneLaunch)
            {
                //Auto queue to matchmaker in dev 
                matchmakerQueue.Dispatch(new QueueModel(), key);
            }
        }

        //Get rid of all the junk in the editor scene
        void Cleanup()
        {
            //remove any minions on the board for a clean slate
            piecesModel.Pieces = new List<PieceModel>();
            var taggedPieces = GameObject.FindGameObjectsWithTag("Piece");
            foreach (var piece in taggedPieces)
            {
                GameObject.Destroy(piece);
            }

            //clean up scene cards, init lists.  Need better place for init
            var taggedCards = GameObject.FindGameObjectsWithTag("Card");
            foreach (var card in taggedCards)
            {
                GameObject.Destroy(card);
            }

            var goMap = GameObject.Find("Map");
            if (goMap != null)
            {
                GameObject.Destroy(goMap);
            }

#if !DEBUG
            var dbgButtons = GameObject.Find("DebugButtons");
            GameObject.DestroyImmediate(dbgButtons);
#endif
        }
    }
}


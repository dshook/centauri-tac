import loglevel from 'loglevel-decorator';
import GameHost from './GameHost.js';
import PlayerDeck from 'models/PlayerDeck';

/**
 * Manages all the game hosts and adding and removing clients from them
 * as well as other operations like completing and changing state
 */
@loglevel
export default class GameManager
{
  constructor(games, decks, emitter, componentsConfig)
  {
    this.games = games;
    this.decks = decks;
    this.emitter = emitter;
    this.componentsConfig = componentsConfig;

    this.gameHosts = [];
    // all {client, playerId, gameId} across all games
    this.clients = [];
  }

  /**
   * Start new game instance based on info
   */
  async create(name, expectedPlayers)
  {
    // registers the game

    //what map to play on if forced, prob should be move to a config at some point
    const map = process.env.MAP || 'cubeland';
    const game = await this.games.create(
      name,
      map,
      2,
      this.componentsConfig.turnLengthMs,
      this.componentsConfig.turnEndBufferLengthMs,
      this.componentsConfig.turnIncrementLengthMs
    );

    if(game == null){
      this.log.error('Could not create game %s', name);
      return null;
    }

    this.log.info('Created game %s %s, expectedPlayers %j', game.id, game.name, expectedPlayers);

    // look up the players decks they joined with to share with the game
    let deckInfo = await Promise.all(expectedPlayers.map(
      async p => {
        if(!p.deckId){
          //testing for now joining with no deck specified, create a placeholder
          return PlayerDeck.fromData({
            playerId: p.playerId,
            race: p.playerId % 2 === 0 ? 2 : 3
          });
        }
        let deck = await this.decks.getDecks(p.playerId, p.deckId);
        return deck[0];
      }
    ));

    this.log.info('Got Deck info for expected players %j', deckInfo);

    // instantiates game on the game host
    await this.emitter.emit('game', game);

    const host = new GameHost(game, deckInfo, this.emitter);
    await host.startInstance();
    this.gameHosts.push(host);

    this.log.info('game instance for %s started!', game.id);

    // change the game state to staging
    await this.setGameState(game.id, 2);

    return game;
  }


  /**
   * Drop a player into a running game host
   */
  async playerJoin(playerId, gameId, deckId)
  {
    const currentGameId = await this.games.currentGameId(playerId);

    // if player is already in a game, KICK EM OUT
    if (currentGameId && currentGameId !== gameId) {
      this.log.warn('player %s already in %s, kicking', playerId, gameId);
      await this.playerPart(playerId);
    }

    // insert to DB
    await this.games.playerJoin(playerId, gameId, deckId);

    // Fire update events
    const game = await this.games.getActive(gameId);
    await this.emitter.emit('game:current', {game, playerId});
    await this.emitter.emit('game', game);
  }

  /**
   * Player client requesting to join the game
   */
  async playerJoinGame(client, playerId, gameId)
  {
    const host = this._getHost(gameId);
    if(!host){
      //TODO: temp for now while there's only one server running, remove an old game so next run it'll work
      this.log.warn('No host for player to join game on, cleaning up', gameId);
      await this.completeGame(gameId, null);
      return;
    }

    if (!(await host.canPlayerJoin(client, playerId))) {
      throw new Error('cannot join when allowJoin is false');
    }

    this.log.info('player %s joining game %s', playerId, gameId);

    // master list
    this.clients.push({client, playerId, gameId});

    // let the host do its thang
    host.addClient(client, playerId);
  }

  /**
   * A client either disconnecting or requesting to part from their game, if they're in one
   */
  async playerPart(client)
  {
    const index = this.clients.findIndex(x => x.client === client);

    if (!~index) {
      //This is a benign message since this function is called in a player part or disconnect
      //so if the client sends a part and then disconnects themselves this will be called twice,
      //the second time the client won't be in our list
      this.log.info('client %s not in list', client ? client.id : -1);
      return;
    }

    const clientListing = this.clients[index];
    const {gameId, playerId} = clientListing;

    // remove from master list
    this.clients.splice(index, 1);

    this.log.info('client %s parting game %s', client.id, gameId);

    // remove from host
    const host = this._getHost(gameId);
    if(host){
      host.dropClient(client, playerId);
    }

    this.emitter.emit('playerParted', {gameId, playerId});

    await this.games.playerPart(playerId, gameId);

    await this.emitter.emit('game:current', {game: null, playerId});

    const game = await this.games.getActive(gameId);

    // no longer active (zombie game)
    if (!game) {
      await this.completeGame(game.id, null);
      return;
    }

    // Otherwise see if there are any players left in the game and give them the w
    let gamePlayers = await this.games.playersInGame(gameId);

    let winner = (gamePlayers || []).find(p => p.id !== playerId);

    await this.completeGame(game.id, winner ? winner.id : null);

    // Broadcast updated model
    await this.emitter.emit('game', game);

    // axe the connection finally (if it was a part)
    // if it was a part this function will be called again since it's wired up to the DC but the client
    // won't be in the list by that point so we're good
    client.disconnect();
  }

  clientDisconnect(client){
    this.log.info('Client %s disconnected', client ? client.id : -1);
  }

  /**
   * Complete a game and assign a winner
   */
  async completeGame(gameId, winningPlayerId = null)
  {
    this.log.info('completing game %s with winner %s', gameId, '' + winningPlayerId);

    // remove from registry
    await this.games.complete(gameId, winningPlayerId);

    // kill the game on the server
    const index = this.gameHosts.findIndex(x => x.game.id === gameId);

    // possible a shutdown message would happen before the game was actually
    // started, so dont barf over it
    if (!~index) {
      this.log.info('game %s isnt running on this component', gameId);
      return;
    }

    const host = this.gameHosts[index];

    //Tell the clients who won
    for (const player of [...host.players]) {
      player.client.send('game:finished', {
          id: 99999,
          winnerId: winningPlayerId,
        }
      );
    }

    // remove all remaining players
    this.log.info('removing remaining players');
    for (const player of [...host.players]) {
      this.playerPart(player.client, player.id);
    }

    await host.shutdown();

    this.gameHosts.splice(index, 1);
    this.log.info('shutdown %s and removed from gameHosts list, %d still running',
        gameId, this.gameHosts.length);
  }

  /**
   * Change the run state of a game
   */
  async setGameState(gameId, stateId)
  {
    await this.games.setState(gameId, stateId);

    // broadcast updated game info
    const game = await this.games.getActive(gameId);
    await this.emitter.emit('game', game);
  }

  /**
   * Change the allow join flag of a game
   */
  async setAllowJoin(gameId, allow = true)
  {
    await this.games.setAllowJoin(gameId, allow);

    // broadcast updated game info
    const game = await this.games.getActive(gameId);
    await this.emitter.emit('game', game);
  }

  /**
   * Broadcast the current game for a certain player
   */
  async getCurrentGame(playerId)
  {
    const gameId = await this.games.currentGameId(playerId);

    let game = null;
    if (gameId) {
      game = await this.games.getActive(gameId);
    }

    return game;
  }

  /**
   * gameId -> host instance
   */
  _getHost(gameId)
  {
    const host = this.gameHosts.find(x => x.game.id === gameId);

    if (!host) {
      this.log.error('Could not find host for game %s', gameId);
      return null;
    }

    return host;
  }
}

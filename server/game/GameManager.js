import loglevel from 'loglevel-decorator';
import GameHost from './GameHost.js';

/**
 * Manages all the game hosts and adding and removing clients from them
 * as well as other operations like completing and changing state
 */
@loglevel
export default class GameManager
{
  constructor(games, emitter, componentsConfig)
  {
    this.games = games;
    this.emitter = emitter;
    this.componentsConfig = componentsConfig;

    this.gameHosts = [];
    // all {client, playerId, gameId} across all games
    this.clients = [];
  }

  /**
   * Start new game instance based on info
   */
  async create(name, playerId)
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
      this.log.info('Could not create game for %s component: %s player: %s'
        ,name, playerId);
      return null;
    }

    this.log.info('Created game %s %s', game.id, game.name);

    // instantiates game on the game host
    await this.emitter.emit('game', game);

    // have host join the game (will fire update events)
    await this.playerJoin(playerId, game.id);

    const host = new GameHost(game, this.emitter);
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
  async playerJoin(playerId, gameId)
  {
    const currentGameId = await this.games.currentGameId(playerId);

    // if player is already in a game, KICK EM OUT
    if (currentGameId && currentGameId !== gameId) {
      this.log.warn('player %s already in %s, kicking', playerId, gameId);
      await this.playerPart(playerId);
    }

    // insert to DB
    await this.games.playerJoin(playerId, gameId);

    // Fire update events
    const game = await this.games.getActive(gameId);
    await this.emitter.emit('game:current', {game, playerId});
    //await this.emitter.emit('game', game);
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

    // tell gamelist we've a new player
    // await this.emitter.emit('playerJoined', {gameId, playerId});

    // master list
    this.clients.push({client, playerId, gameId});

    // let the host do its thang
    host.addClient(client, playerId);
  }

  /**
   * Remove a player from a game and kick em off the gamelist
   */
  async playerPart(playerId, gameId)
  {
    this.log.info('dropping player %s from their game', playerId);
    const id = await this.games.playerPart(playerId);

    // not in a game, nothing to do
    if (!id) {
      return;
    }

    await this.emitter.emit('game:current', {game: null, playerId});

    const game = await this.games.getActive(id);

    // no longer active (zombie game)
    if (!game) {
      await this.completeGame(game.id, null);
      return;
    }

    // empty game, kill it
    else if (!game.currentPlayerCount) {
      this.log.info('after %s parted, game is empty', playerId);
      await this.completeGame(game.id, null);
      return;
    }

    // Otherwise just broadcast updated model
    await this.emitter.emit('game', game);
  }

  /**
   * A client requesting to part from their game
   */
  async playerPartGame(client, playerId)
  {
    const index = this.clients.findIndex(x => x.client === client);

    if (!~index) {
      this.log.info('problem finding client %s player %s in list', client ? client.id : -1, playerId);
      return;
    }

    const clientListing = this.clients[index];
    const {gameId} = clientListing;
    playerId = clientListing.playerId;

    this.log.info('player %s parting game %s', playerId, gameId);

    // remove from host
    const host = this._getHost(gameId);
    if(host){
      host.dropClient(client, playerId);
    }

    // remove from master list
    this.clients.splice(index, 1);

    this.emitter.emit('playerParted', {gameId, playerId});
  }


  /**
   * Complete a game and assign a winner
   */
  async completeGame(gameId, winningPlayerId = null)
  {
    this.log.info('completing game %s with winner %s', gameId, '' + winningPlayerId);

    const players = await this.games.playersInGame(gameId);

    // kick all players
    for (const p of players) {
      await this.playerPart(p.id);
    }

    // kill the game on the server
    //const game = await this.games.getActive(gameId);
    await this.shutdown(gameId);

    // remove from registry
    await this.games.complete(gameId, winningPlayerId);

    // announce
    await this.emitter.emit('game:remove', gameId);
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
   * Destroy a running game by id
   */
  async shutdown(gameId)
  {
    const index = this.gameHosts.findIndex(x => x.game.id === gameId);

    // possible a shutdown message would happen before the game was actually
    // started, so dont barf over it
    if (!~index) {
      this.log.info('game %s isnt running on this component', gameId);
      return;
    }

    const host = this.gameHosts[index];

    // remove all players
    this.log.info('removing all players');
    for (const player of [...host.players]) {
      this.playerPartGame(player.client, player.id);
    }

    await host.shutdown();

    this.gameHosts.splice(index, 1);
    this.log.info('shutdown %s and removed from gameHosts list, %d still running',
        gameId, this.gameHosts.length);
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

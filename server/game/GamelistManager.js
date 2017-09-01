import loglevel from 'loglevel-decorator';

/**
 * Handles the game registry. Cordinates with the game store, net client, and
 * event bus to deal with all high-level game registry stuff.
 */
@loglevel
export default class GamelistManager
{
  constructor(games, emitter, componentManager, gameManager, componentsConfig)
  {
    this.games = games;
    this.emitter = emitter;
    this.componentManager = componentManager;
    this.gameManager = gameManager;
    this.componentsConfig = componentsConfig;
  }

  /**
   * Add a new game to the registry (and spin it up on a game component)
   */
  async createNewGame(name, playerId)
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

    this.log.info('Gamelist created game %s %s', game.id, game.name);

    // instantiates game on the game host
    await this.emitter.emit('game:created', game);
    await this.emitter.emit('game', game);

    // have host join the game (will fire update events)
    await this.playerJoin(playerId, game.id);

    return game;
  }

  /**
   * Remove a player from a game
   */
  async playerPart(playerId)
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
   * Add a player to a game
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
    await this.emitter.emit('game', game);
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
    await this.gameManager.shutdown(gameId);

    // remove from registry
    await this.games.complete(gameId, winningPlayerId);

    // announce
    await this.emitter.emit('game:remove', gameId);
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
}

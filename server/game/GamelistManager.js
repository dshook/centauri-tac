import loglevel from 'loglevel-decorator';

/**
 * Handles the game registry. Cordinates with the game store, net client, and
 * event bus to deal with all high-level game registry stuff.
 */
@loglevel
export default class GamelistManager
{
  constructor(messenger, games, netClient)
  {
    this.messenger = messenger;
    this.games = games;
    this.net = netClient;
  }

  /**
   * Add a new game to the registry (and spin it up on a game component)
   */
  async createNewGame(name, playerId)
  {
    // get an available game server
    const component = await this.net.getComponent('game');

    // registers the game
    const game = await this.games.create(name, component.id, playerId);

    if(game == null){
      this.log.info('Could not create game for %s component: %s player: %s'
        ,name, component.id, playerId);
      return null;
    }

    // instantiates game on the game host
    await this.net.post(component, 'game', game);

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

    await this.messenger.emit('game:current', {game: null, playerId});

    const game = await this.games.getActive(null, id);

    // no longer active (zomebie game)
    if (!game) {
      await this.removeGame(game.id);
      return;
    }

    // empty game, kill it
    else if (!game.currentPlayerCount) {
      this.log.info('after %s parted, game is empty', playerId);
      await this.removeGame(game.id);
      return;
    }

    // host left
    else if (game.hostPlayerId === playerId) {
      await this.assignNewHost(game.id);
      return;
    }

    // Otherwise just broadcast updated model
    await this.messenger.emit('game', game);
  }

  /**
   * Add a player to a game
   */
  async playerJoin(playerId, gameId)
  {
    const currentGameId = await this.games.currentGameId(playerId);

    // if player is already in a game, KICK EM OUT
    if (currentGameId && currentGameId !== gameId) {
      this.log.info('player %s already in %s, kicking', playerId, gameId);
      await this.playerPart(playerId);
    }

    // insert to DB
    await this.games.playerJoin(playerId, gameId);

    // Fire update events
    const game = await this.games.getActive(null, gameId);
    await this.messenger.emit('game:current', {game, playerId});
    await this.messenger.emit('game', game);
  }

  /**
   * Change the run state of a game
   */
  async setGameState(gameId, stateId)
  {
    await this.games.setState(gameId, stateId);

    // broadcast updated game info
    const game = await this.games.getActive(null, gameId);
    await this.messenger.emit('game', game);
  }

  /**
   * Change the allow join flag of a game
   */
  async setAllowJoin(gameId, allow = true)
  {
    await this.games.setAllowJoin(gameId, allow);

    // broadcast updated game info
    const game = await this.games.getActive(null, gameId);
    await this.messenger.emit('game', game);
  }

  /**
   * Drop a game out
   */
  async removeGame(gameId)
  {
    this.log.info('removing game %s', gameId);

    const players = await this.games.playersInGame(gameId);

    // kick all players
    for (const p of players) {
      await this.playerPart(p.id);
    }

    // kill the game on the server
    const game = await this.games.getActive(null, gameId);
    await this.net.post(game.component, 'game/shutdown', {gameId: game.id});

    // remove from registry
    await this.games.remove(gameId);

    // announce
    await this.messenger.emit('game:remove', gameId);
  }

  /**
   * Assign a new host to a game
   */
  async assignNewHost(gameId)
  {
    const players = await this.games.playersInGame(gameId);

    // just pick the first one
    const p = players[0];

    await this.games.setHost(gameId, p.id);

    // broadcast updated game info via gamelist
    const game = await this.games.getActive(null, gameId);
    await this.messenger.emit('game', game);
  }

  /**
   * Broadcast the current game for a certain player
   */
  async getCurrentGame(playerId)
  {
    const gameId = await this.games.currentGameId(playerId);

    let game = null;
    if (gameId) {
      game = await this.games.getActive(null, gameId);
    }

    return game;
  }

  /**
   * Player was the host of game but parted it
   */
  async _handleHostLeft(game)
  {
    this.log.info('host left');

    // game was total empty, time to remove it
    if (game.currentPlayerCount === 0) {
      this.log.info('...and was last one');

      await this.removeGame(game.id);
      return;
    }

    // not empty yet
    await this.assignHost(game.id);
  }
}

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

    // instantiates game on the game host
    await this.net.post(component, 'game', game);

    // have host join the game (will fire update events)
    await this.playerJoin(playerId, game.id);
  }

  /**
   * Remove a player from a game
   */
  async playerPart(playerId)
  {
    const id = await this.games.playerPart(playerId);

    // not in a game, nothing to do
    if (!id) {
      return;
    }

    await this.messenger.emit('game:current', {game: null, playerId});

    const game = await this.games.getActive(null, id);

    // no longer active (zomebie game)
    if (!game) {
      this.remove(game.id);
      return;
    }

    // empty game, kill it
    else if (!game.currentPlayerCount) {
      this.removeGame(game.id);
    }

    // host left
    else if (game.hostPlayerId === playerId) {
      this.assignNewHost(game.id);
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

    // if player is already in a game, just barf
    if (currentGameId) {
      throw new Error('Player is already in a game');
    }

    // insert to DB
    this.games.playerJoin(playerId, gameId);

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
   * Drop a game out
   */
  async removeGame(gameId)
  {
    const players = await this.games.playersInGame(gameId);

    // kick all players
    for (const p of players) {
      await this.playerPart(p.id);
    }

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

    // broadcast updated game info
    const game = await this.games.getActive(null, gameId);
    await this.messenger.emit('game', game);
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

      // and never started by teh game server
      if (game.state === null) {
        this.log.info('...and was never started on the server');
      }
      else {
        // wipe game instance
        await this.net.post(game.component, 'game/shutdown', {gameId: game.id});
      }

      await this.remove(game.id);
      return;
    }

    // not empty yet
    await this.assignHost(game.id);
  }
}

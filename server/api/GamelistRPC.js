import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';
import {on} from 'emitter-binder';

/**
 * RPC handler for the gamelist component
 */
@loglevel
export default class GamelistRPC
{
  constructor(games, gamelistManager, componentsConfig)
  {
    this.manager = gamelistManager;
    this.games = games;
    this.realm = componentsConfig.realm;

    this.clients = new Set();
  }

  /**
   * Client wants the entire gamelist
   */
  @rpc.command('gamelist')
  @rpc.middleware(roles(['player']))
  async sendGamelist(client)
  {
    const games = await this.games.getActive(this.realm);
    for (const g of games) {
      client.send('game', g);
    }
  }

  /**
   * A game component is informing us of a state change
   */
  @on('update:state')
  async updateState(client, {gameId, stateId})
  {
    await this.manager.setGameState(gameId, stateId);
  }

  /**
   * A game component is switching the allow join param
   */
  @on('update:allowJoin')
  async updateAllowJoin(client, {gameId, allowJoin})
  {
    await this.manager.setAllowJoin(gameId, allowJoin);
  }

  /**
   * Client wants to create a game
   */
  @on('create')
  async createGame(client, {name}, auth)
  {
    const playerId = auth.sub.id;
    await this.manager.createNewGame(name, playerId);
  }

  /**
   * Component is building a game for a set of players
   */
  @on('createFor')
  async createGameFor(client, {name, playerIds})
  {
    const [host, ...others] = playerIds;

    const game = await this.manager.createNewGame(name, host);

    for (const id of others) {
      await this.manager.playerJoin(id, game.id);
    }
  }

  /**
   * A game component is informing us that a player has joined
   */
  @on('playerJoined')
  async playerJoined(client, {gameId, playerId})
  {
    this.manager.playerJoin(playerId, gameId);
  }

  /**
   * A game component is informing us that a player has parted
   */
  @on('playerParted')
  async playerParted(client, {gameId, playerId})
  {
    this.manager.playerPart(playerId, gameId);
  }

  /**
   * Send game update to all connected
   */
  @on('game')
  _broadcastGame(game)
  {
    for (const c of this.clients) {
      c.send('game', game);
    }
  }

  /**
   * Send game remove to all connected
   */
  @on('game:remove')
  _broadcastRemoveGame(gameId)
  {
    for (const c of this.clients) {
      c.send('game:remove', gameId);
    }
  }

  /**
   * If a player is conencted, inform them of their current game
   */
  @on('game:current')
  _broadcastCurrentGame({game, playerId})
  {
    for (const c of this.clients) {
      const {id} = c.auth.sub;
      if (playerId === id) {
        c.send('game:current', game);
      }
    }
  }

  /**
   * When a client connects
   */
  @rpc.command('_token')
  async hello(client, params, auth)
  {
    // if no user, this is a component, and we dont need to track for
    // broadcasts?
    if (!auth.sub) {
      return;
    }

    this.clients.add(client);

    const playerId = auth.sub.id;

    // player already in a game?
    const gId = await this.games.currentGameId(playerId);

    if (!gId) {
      return;
    }

    const game = await this.games.getActive(null, gId);

    // inform player of current game
    client.send('game:current', game);
  }

  /**
   * Track connected clients
   */
  @rpc.disconnected()
  bye(client)
  {
    this.clients.delete(client);
  }
}

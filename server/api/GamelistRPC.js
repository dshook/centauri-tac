import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import {dispatch} from 'rpc-messenger';
import roles from '../middleware/rpc/roles.js';

/**
 * RPC handler for the gamelist component
 */
@loglevel
export default class GamelistRPC
{
  constructor(games, componentsConfig, netClient)
  {
    this.games = games;
    this.realm = componentsConfig.realm;
    this.net = netClient;

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
   * Client wants to create a game
   */
  @rpc.command('create')
  @rpc.middleware(roles(['player']))
  async createGame(client, {name}, auth)
  {
    const playerId = auth.sub.id;

    // get an available game server
    const component = await this.net.getComponent('game');

    // registers the game
    const game = await this.games.create(name, component.id, playerId);

    // instantiates game on the game host
    await this.net.post(component, 'game', game);

    // have host join the game (will fire update events)
    await this.games.playerJoin(playerId, game.id);
  }

  /**
   * Client wants to leave a game
   */
  @rpc.command('part')
  @rpc.middleware(roles(['player']))
  async playerPart(client, params, auth)
  {
    const playerId = auth.sub.id;
    await this.games.playerPart(playerId);
  }

  /**
   * Client wants to join a game
   */
  @rpc.command('join')
  @rpc.middleware(roles(['player']))
  async playerJoin(client, {gameId}, auth)
  {
    const playerId = auth.sub.id;
    await this.games.playerJoin(playerId, gameId);
  }

  /**
   * Send game update to all connected
   */
  @dispatch.on('game')
  _broadcastGame(game)
  {
    for (const c of this.clients) {
      c.send('game', game);
    }
  }

  /**
   * Send game remove to all connected
   */
  @dispatch.on('game:remove')
  _broadcastRemoveGame(gameId)
  {
    for (const c of this.clients) {
      c.send('game:remove', gameId);
    }
  }

  /**
   * If a player is conencted, inform them of their current game
   */
  @dispatch.on('game:current')
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
    this.clients.add(client);

    const playerId = auth.sub.id;

    // player already in a game?
    const gId = await this.games.currentGameId(playerId);

    if (!gId) {
      return;
    }

    const game = await this.games.getActive(null, gId);

    // if we didnt get one back, means the game is no longer on an active
    // server instnace (zombie game, so delete it and move on
    if (!game) {
      await this.games.playerPart(playerId);
      this.log.info('kicked player %s from game %s, on an inactive server',
          playerId, gId);
      return;
    }

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

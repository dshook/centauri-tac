import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import {dispatch} from 'rpc-messenger';

/**
 * RPC handler for the gamelist component
 */
@loglevel
export default class GamelistRPC
{
  constructor(messenger, games, componentsConfig, netClient)
  {
    this.games = games;
    this.realm = componentsConfig.realm;
    this.net = netClient;

    messenger.bindInstance(this);

    setTimeout(() => this.net.sendCommand('dispatch', 'subscribe', 'game'), 500);

    this.clients = new Set();
  }

  /**
   * Client wants the entire gamelist
   */
  @rpc.command('gamelist')
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
  async createGame(client, {name}, auth)
  {
    const playerId = auth.sub.id;

    // get an available game server
    const component = await this.net.getComponent('game');

    await this.games.create(name, component.id, playerId);
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
   * If a player is conencted, inform them of their current game
   */
  @dispatch.on('game:current')
  _broadcastCurrentGame({game, playerId})
  {
    for (const c of this.clients) {
      const id = c.auth.sub;
      if (playerId === id) {
        c.send('currentGame', game);
      }
    }
  }

  /**
   * Track connected clients
   */
  @rpc.connected()
  hello(client)
  {
    this.clients.add(client);
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

import {rpc} from 'sock-harness';
import {autobind} from 'core-decorators';
import loglevel from 'loglevel-decorator';

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

    this.games.on('game', this._broadcastGame);
  }

  /**
   * Send gamelist to a client
   */
  @rpc.command('gamelist')
  async getGamelist(client)
  {
    const games = await this.games.getActive(this.realm);
    for (const g of games) {
      client.send('game', g);
    }
  }

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
  @autobind _broadcastGame(game)
  {
    for (const c of this.clients) {
      c.send('game', game);
    }
  }

  @rpc.connected()
  hello(client)
  {
    this.clients.add(client);
    this.log.info('%s connected, %s total', client.id, this.clients.size);
  }

  @rpc.disconnected()
  bye(client)
  {
    this.clients.delete(client);
    this.log.info('%s disconnected, %s total', client.id, this.clients.size);
  }
}

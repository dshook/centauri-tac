import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';

/**
 * RPC handler for the gamelist component
 */
@loglevel
export default class GamelistRPC
{
  constructor(games, componentsConfig, netClient, messenger)
  {
    this.games = games;
    this.realm = componentsConfig.realm;
    this.net = netClient;
    this.messenger = messenger;

    this.clients = new Set();
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

    // TODO: Handle this another way, need some sort of manager that will
    // intelligent get a game instance get an available game server
    const component = await this.net.getComponent('game');

    await this.games.create(name, component.id, playerId);
  }
}

import {rpc} from 'sock-harness';

/**
 * RPC handler for the gamelist component
 */
export default class GamelistRPC
{
  constructor(games, componentsConfig, netClient)
  {
    this.games = games;
    this.realm = componentsConfig.realm;
    this.net = netClient;
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
}

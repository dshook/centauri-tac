import loglevel from 'loglevel-decorator';
import GameHost from './GameHost.js';

/**
 * Manages all the game hosts and adding and removing clients from them
 */
@loglevel
export default class GameManager
{
  constructor(netClient, gameInstanceFactory)
  {
    this.hosts = [];
    this.factory = gameInstanceFactory;
    this.net = netClient;
  }

  /**
   * Start new game instance based on info
   */
  async create(game)
  {
    this.log.info('creating new game %s %s', game.id, game.name);

    const instance = this.factory();

    const host = new GameHost(game, instance);

    this.hosts.push(host);

    // inform server our state has changed to staging
    const gameId = game.id;
    const stateId = 2;
    await this.net.sendCommand('gamelist', 'update:state', {gameId, stateId});
  }

  /**
   * Destroy a running game by id
   */
  async shutdown(gameId)
  {
    const index = this.hosts.findIndex(x => x.game.id === gameId);

    if (!~index) {
      throw new Error(`could not find game ${gameId}`);
    }

    this.hosts[index].shutdown();
    this.hosts.splice(index, 1);
    this.log.info('shutdown %s and removed from hosts list', gameId);
  }
}

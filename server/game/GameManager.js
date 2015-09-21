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

    // all {client, playerId, gameId} across all games
    this.clients = [];
  }

  /**
   * Drop a player into a running game host
   */
  async playerJoin(client, playerId, gameId)
  {
    this.log.info('player %s joining game %s', playerId, gameId);
    const host = this._getHost(gameId);

    // TODO: confirm with gamelist that this player can join our game

    // master list
    this.clients.push({client, playerId, gameId});

    // let the host do its thang
    host.addClient(client, playerId);
  }

  /**
   * Remove a player from a game and kick em off the gamelist
   */
  async playerPart(client, playerId)
  {
    const index = this.clients.findIndex(x => x.client === client);
    const {gameId} = this.clients[index];

    this.log.info('player %s parting game %s', playerId, gameId);

    // remove from host
    const host = this._getHost(gameId);
    host.dropClient(client, playerId);

    // remove from master list
    this.clients.splice(index, 1);

    this.net.sendCommand('gamelist', 'playerParted', {gameId, playerId});
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

  _getHost(gameId)
  {
    const index = this.hosts.findIndex(x => x.game.id === gameId);

    if (!~index) {
      throw new Error(`could not find game ${gameId}`);
    }

    return this.hosts[index];
  }
}

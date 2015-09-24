import loglevel from 'loglevel-decorator';
import GameHost from './GameHost.js';

/**
 * Manages all the game hosts and adding and removing clients from them
 */
@loglevel
export default class GameManager
{
  constructor(netClient, gameModules)
  {
    this.hosts = [];
    this.net = netClient;
    this.modules = gameModules;

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

    // tell gamelist we've a new player
    await this.net.sendCommand('gamelist', 'playerJoined', {gameId, playerId});

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

    if (!~index) {
      throw new Error('problem finding client %s player %s in list',
          client.id, playerId);
    }

    const {gameId} = this.clients[index];

    this.log.info('player %s parting game %s', playerId, gameId);

    // remove from host
    const host = this._getHost(gameId);
    host.dropClient(client, playerId);

    // remove from master list
    this.clients.splice(index, 1);

    await this.net.sendCommand('gamelist', 'playerParted', {gameId, playerId});
  }

  /**
   * Start new game instance based on info
   */
  async create(game)
  {
    this.log.info('creating new game %s %s', game.id, game.name);

    const host = new GameHost(game, this.modules);
    await host.startInstance();
    this.hosts.push(host);

    this.log.info('game instance for %s started!', game.id);

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

    // possible a shutdown message would happen before the game was actually
    // started, so dont barf over it
    if (!~index) {
      this.log.info('game %s isnt running on this component', gameId);
      return;
    }

    const host = this.hosts[index];

    // remove all players
    this.log.info('removing all players');
    for (const player of [...host.players]) {
      this.playerPart(player.client, player.id);
    }

    await host.shutdown();

    this.hosts.splice(index, 1);
    this.log.info('shutdown %s and removed from hosts list, %d still running',
        gameId, this.hosts.length);
  }

  /**
   * gameId -> host instance
   */
  _getHost(gameId)
  {
    const index = this.hosts.findIndex(x => x.game.id === gameId);

    if (!~index) {
      throw new Error(`could not find game ${gameId}`);
    }

    return this.hosts[index];
  }
}

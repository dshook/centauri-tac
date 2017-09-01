import loglevel from 'loglevel-decorator';
import GameHost from './GameHost.js';

/**
 * Manages all the game hosts and adding and removing clients from them
 */
@loglevel
export default class GameManager
{
  constructor(emitter)
  {
    this.hosts = [];
    this.emitter = emitter;

    // all {client, playerId, gameId} across all games
    this.clients = [];
  }

  /**
   * Drop a player into a running game host
   */
  async playerJoin(client, playerId, gameId)
  {
    const host = this._getHost(gameId);
    if(!host) return;

    if (!(await host.canPlayerJoin(client, playerId))) {
      throw new Error('cannot join when allowJoin is false');
    }

    this.log.info('player %s joining game %s', playerId, gameId);

    // tell gamelist we've a new player
    await this.emitter.emit('playerJoined', {gameId, playerId});

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
      this.log.info('problem finding client %s player %s in list', client ? client.id : -1, playerId);
      return;
    }

    const clientListing = this.clients[index];
    const {gameId} = clientListing;
    playerId = clientListing.playerId;

    this.log.info('player %s parting game %s', playerId, gameId);

    // remove from host
    const host = this._getHost(gameId);
    if(host){
      host.dropClient(client, playerId);
    }

    // remove from master list
    this.clients.splice(index, 1);

    this.emitter.emit('playerParted', {gameId, playerId});
  }

  /**
   * Start new game instance based on info
   */
  async create(game)
  {
    this.log.info('creating new game %s %s turn length %s', game.id, game.name, game.turnLengthMs);

    const host = new GameHost(game, this.emitter);
    await host.startInstance();
    this.hosts.push(host);

    this.log.info('game instance for %s started!', game.id);

    // inform server our state has changed to staging
    const gameId = game.id;
    const stateId = 2;
    await this.emitter.emit('game:updatestate', {gameId, stateId});
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
    const host = this.hosts.find(x => x.game.id === gameId);

    if (!host) {
      this.log.error('Could not find host for game %s, Hosts registered', gameId, this.hosts);
      return null;
    }

    return host;
  }
}

import loglevel from 'loglevel-decorator';
import Player from 'models/Player';
import {AggregateBinder} from 'emitter-binder';
import {EventEmitter} from 'events';
import {on} from 'emitter-binder';
import Application from 'billy';
import CentauriTacGame from '../game/ctac/CentauriTacGame.js';
import HostManager from '../game/HostManager.js';
import GameConfig from './GameConfig.js';

// TODO: in configs
import ActionQueueService from './ctac/services/ActionQueueService.js';
import GameDataService from './ctac/services/GameDataService.js';
import ProcessorsService from './ctac/services/ProcessorsService.js';
import CardService from './ctac/services/CardService.js';
import MapService from './ctac/services/MapService.js';
import UtilService from './ctac/services/UtilService.js';
import GameEventService from './ctac/services/GameEventService.js';

/**
 * Top-level entity for a running game
 */
@loglevel
export default class GameHost extends EventEmitter
{
  constructor(game, emitter)
  {
    super();

    this.log.info('created new GameHost for game %s', game.id);
    this.game = game;
    this.emitter = emitter;

    // Relay emitted events to instance
    this.binder = new AggregateBinder();

    // Also let this class be a listener AND emitter lmbo
    this.binder.bindInstance(this);
    this.binder.addEmitter(this);

    this.players = [];
  }

  /**
   * Create a new game instance
   */
  async startInstance()
  {
    const app = new Application();

    this.log.info('booting up game stack for game %s', this.game.id);

    // injectables
    app.registerInstance('gameConfig', new GameConfig());
    app.registerInstance('players', this.players);
    app.registerInstance('binder', this.binder);
    app.registerInstance('game', this.game);

    const manager = new HostManager(app, this.binder, this.game, this.emitter);
    app.registerInstance('host', manager);

    // TODO: pull this out to configs
    app.service(ActionQueueService);
    app.service(MapService);
    app.service(GameDataService);
    app.service(CardService);
    app.service(GameEventService);
    app.service(ProcessorsService);
    app.service(UtilService);

    await app.start();

    // TODO: pull this out to configs
    manager.addController(CentauriTacGame);
  }

  /**
   * Determine if we should let a player join this game
   */
  async canPlayerJoin(client, playerId)
  {
    const inGame = this.players.some(x => x.id === playerId);
    return inGame || this.game.allowJoin;
  }

  /**
   * Re-emit command with player object instead of client so handlers have
   * access to the player
   */
  @on('command')
  onClientCommand({command, params}, client)
  {
    const player = this.players.find(x => x.client === client);
    this.emit('playerCommand', command, params, player);
  }

  /**
   * Manager gives us a new friend (player joining the game)
   */
  async addClient(client, playerId)
  {
    this.log.info('client %s has connected for player %s on host for game %s',
        client.id, playerId, this.game.id);

    let player = this.players.find(x => x.id === playerId);

    // add the client to the binder so it sends events to all listeners
    this.binder.addEmitter(client);

    // hello
    client.send('join');

    // Events with the connection is broken
    client.once('close', () => this._clientClose(player));

    // send all current players to the client
    for (const p of this.players) {
      client.send('player:connect', p);
      client.send('player:join', p);
    }

    if (player) {
      this.log.info('player %s has reconnected!', player.id);
      player.client = client;

      this.emit('playerConnected', player);
      await this._broadcastCommand('player:connect', player);
    }
    else {
      this.log.info('creating new player instance for %s', playerId);
      player = Player.fromClient(client);
      this.players.push(player);

      this.emit('playerConnected', player);
      await this._broadcastCommand('player:connect', player);
      this.emit('playerJoined', player);
      await this._broadcastCommand('player:join', player);
    }

  }

  /**
   * Manager is taking away a friend (player leaving game)
   */
  async dropClient(client)
  {
    this.log.info('client %s is leaving host for game %s',
        client.id, this.game.id);

    const player = this.players.find(x => x.client === client);

    if (!player) {
      throw new Error('no player found with client %s', client.id);
    }

    await this._broadcastCommand('player:part', player);

    // hello
    client.send('part');

    // clear out on purpose
    this.binder.removeEmitter(player.client);
    player.client = null;
    client.disconnect();
  }

  /**
   * Disconnect happened, either manually or from client
   */
  _clientClose(player)
  {
    const index = this.players.indexOf(player);

    if (!~index) {
      throw new Error('player was not in the list');
    }

    if (player.client) {
      this.log.info('unexpected client disconnect! %s DC Timeout %s', player.id, process.env.DISCONNECT_TIMEOUT);
      this.binder.removeEmitter(player.client);
      player.client = null;
      setTimeout(() => {
        this.emitter.emit('playerParted', {gameId: this.game.id, playerId: player.id});
      }, process.env.DISCONNECT_TIMEOUT);
    }
    else {
      // remove for good
      this.log.info('clearing player from list');
      this.players.splice(index, 1);
    }

    this.log.info('player %s has disconnected from host for game %s',
        player.id, this.game.id);

    this.emit('playerDisconnected', player);
    this._broadcastCommand('player:disconnect', player);
  }

  /**
   * Sends a param to all connected players
   */
  async _broadcastCommand(command, params)
  {
    for (const player of this.players) {
      if (!player.client) {
        continue;
      }

      await player.client.send(command, params);
    }
  }

  /**
   * We are dying
   */
  async shutdown()
  {
    this.log.info('shutting down GameHost for game %s', this.game.id);
    this.emit('shutdown');

    // TODO: drop all players, trigger something in gamehost
  }
}

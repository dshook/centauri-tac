import loglevel from 'loglevel-decorator';
import Player from 'models/Player';
import {AggregateBinder} from 'emitter-binder';
import {EventEmitter} from 'events';
import {on} from 'emitter-binder';
import Application from 'billy';
import CentauriTacGame from '../game/ctac/CentauriTacGame.js';
import HostManager from '../game/HostManager.js';
import GameConfig from './GameConfig.js';

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
  constructor(game, deckInfo, emitter)
  {
    super();

    this.log.info('created new GameHost for game %s', game.id);
    this.game = game;
    this.deckInfo = deckInfo;
    this.emitter = emitter;

    // Relay emitted events to instance
    this.binder = new AggregateBinder();

    // Also let this class be a listener AND emitter lmbo
    this.binder.bindInstance(this);
    this.binder.addEmitter(this);

    this.players = [];
    this.app = null;
  }

  /**
   * Create a new game instance
   */
  async startInstance()
  {
    let app = this.app = new Application();

    this.log.info('booting up game stack for game %s', this.game.id);

    // injectables
    app.container.registerValue('gameConfig', new GameConfig());
    app.container.registerValue('players', this.players);
    app.container.registerValue('binder', this.binder);
    app.container.registerValue('game', this.game);
    app.container.registerValue('deckInfo', this.deckInfo);

    const hostManager = new HostManager(app.container, this.binder, this.game, this.emitter);
    app.container.registerValue('hostManager', hostManager);

    app.service(ActionQueueService);
    app.service(GameDataService);
    app.service(MapService);
    app.service(CardService);
    app.service(GameEventService);
    app.service(ProcessorsService);
    app.service(UtilService);

    await app.start();

    hostManager.addController(CentauriTacGame);
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
    if(!this.game.allowCommands){
      this.log.warn('Player command %s sent before allowed', command);
      return;
    }

    const player = this.playerByClient(client);
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

    //TODO: Maybe we will need to re-add when a player reconnects?
    if(!player){
      // add the client to the binder so it sends events to all listeners
      this.binder.addEmitter(client);
    }

    // hello
    client.send('join');

    // Hack to give AI the game instance
    if(playerId === -1){
      client.send('gameInstance', this.app);
    }

    // send all current players to the client
    for (const p of this.players) {
      client.send('player:connect', p);
      client.send('player:join', p);
    }

    if (player) { //Shouldn't happen for now before reconnect logic is in
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
    this.log.info('client %s is leaving host in game %s', client.id, this.game.id);

    const player = this.playerByClient(client);

    if (!player) {
      this.log.error('no player found with client %s', client.id);
      return;
    }

    await this._broadcastCommand('player:part', player);

    // hello
    client.send('part');

    // clear out on purpose
    this.binder.removeEmitter(player.client);
    this.players.splice(this.players.indexOf(player), 1);

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
  }

  //TODO: need to cache this in an object or something so this doesn't have to be run for every command
  playerByClient(client){
    return this.players.find(x => x.client === client);
  }
}

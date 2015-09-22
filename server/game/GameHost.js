import loglevel from 'loglevel-decorator';
import Player from 'models/Player';
import {AggregateBinder} from 'emitter-binder';
import {EventEmitter} from 'events';
import {on} from 'emitter-binder';
import Application from 'billy';

/**
 * Top-level entity for a running game
 */
@loglevel
export default class GameHost extends EventEmitter
{
  constructor(game, modules)
  {
    super();

    this.log.info('created new GameHost for game %s', game.id);
    this.game = game;
    this.modules = modules;

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

    this.log.info('booting up game modues for game %s', this.game.id);

    // injectables
    app.registerInstance('players', this.players);
    app.registerInstance('binder', this.binder);
    app.registerInstance('game', this.game);

    // TODO: all of this logic could be replaced if billy had a binding/plugin
    // method that would have a chance to interact with instantiated services
    app.service(async () => {

      const modules = [];

      for (const key in this.modules) {
        const T = this.modules[key];
        this.log.info('creating game module %s via %s', key, T.name);
        const m = app.make(T);
        this.binder.bindInstance(m);
        modules.push(m);
      }

      for (const m of modules) {
        if (typeof m.start === 'function') {
          this.log.info('starting game module %s for game %s',
              m.constructor.name, this.game.id);
          await m.start();
        }
      }

    });

    await app.start();
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

    // add the client to the binder so it sends events to all listeners
    this.binder.addEmitter(client);

    // Events with the connection is broken
    client.once('close', () => this._clientClose(player));
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

    this.emit('playerParting', player);
    await this._broadcastCommand('player:part', player);

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
      this.log.info('unexpected client disconnect!', player.id);
      this.binder.removeEmitter(player.client);
      player.client = null;
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

    // TODO: drop all players, trigger something in gamehost
  }
}

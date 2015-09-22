import loglevel from 'loglevel-decorator';
import Player from 'models/Player';
import EmitterBinder from 'emitter-binder';
import {EventEmitter} from 'events';

/**
 * Top-level entity for a running game
 */
@loglevel
export default class GameHost extends EventEmitter
{
  constructor(game, instance)
  {
    super();

    this.log.info('created new GameHost for game %s', game.id);
    this.game = game;
    this.instance = instance;

    // Relay emitted events to instance
    new EmitterBinder(this).bindInstance(instance);

    this.players = [];
  }

  /**
   * Manager gives us a new friend (player joining the game)
   */
  async addClient(client, playerId)
  {
    this.log.info('client %s has connected for player %s on host for game %s',
        client.id, playerId, this.game.id);

    let entry = this.players.find(x => x.player.id === playerId);

    let player;

    if (entry) {
      player = entry.player;
      this.log.info('player %s has reconnected!', player.id);
      player.client = client;

      this.emit('playerConnected', player);
      await this._broadcastCommand('player:connect', player);
    }
    else {
      this.log.info('creating new player instance for %s', playerId);
      player = Player.fromClient(client);
      entry = {player};
      this.players.push(entry);

      this.emit('playerConnected', player);
      await this._broadcastCommand('player:connect', player);
      this.emit('playerJoined', player);
      await this._broadcastCommand('player:join', player);
    }

    // build a new binder for this client that will relay all events to the
    // game instance
    const binder = new EmitterBinder(client);
    binder.player = player;
    binder.bindInstance(this.instance);
    entry.binder = binder;

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

    const index = this.players.findIndex(x => x.player.client === client);

    if (!~index) {
      throw new Error('no player found with client %s', client.id);
    }

    const {player} = this.players[index];

    this.emit('playerParting', player);
    await this._broadcastCommand('player:part', player);

    // clear out on purpose
    player.client = null;
    client.disconnect();
  }

  /**
   * Disconnect happened, either manually or from client
   */
  _clientClose(player)
  {
    const index = this.players.findIndex(x => x.player === player);

    if (!~index) {
      throw new Error('player was not in the list');
    }

    if (player.client) {
      this.log.info('unexpected client disconnect!', player.id);
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
    for (const {player} of this.players) {
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

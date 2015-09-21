import loglevel from 'loglevel-decorator';
import EmitterBinder from 'emitter-binder';
import Player from 'models/Player';

/**
 * Top-level entity for a running game
 */
@loglevel
export default class GameHost
{
  constructor(game, instance)
  {
    this.log.info('created new GameHost for game %s', game.id);
    this.game = game;
    this.instance = instance;
    this.clients = [];
  }

  /**
   * Manager gives us a new friend
   */
  addClient(client, playerId)
  {
    this.log.info('client %s has connected for player %s on host for game %s',
        client.id, playerId, this.game.id);

    // build a new binder for thsi client that will bind all the
    const binder = new EmitterBinder(client);
    binder.player = Player.fromClient(client);
    binder.bindInstance(this.instance);

    this.clients.push({client, playerId, binder});
    client.once('close', () => this._clientClose(client));
  }

  /**
   * Manager is taking away a friend
   */
  dropClient(client, playerId)
  {
    this.log.info('client %s (player %s) is leaving host for game %s',
        client.id, playerId, this.game.id);

    // TODO: inform the game instance?

    client.disconnect();
  }

  /**
   * A friend is leaving us
   */
  _clientClose(client)
  {
    const index = this.clients.findIndex(x => x.client === client);
    const {binder} = this.clients[index];

    binder.unbindInstance(this.instance);

    this.clients.splice(index, 1);
    this.log.info('client %s has disconnected from host for game %s',
        client.id, this.game.id);
  }

  /**
   * We are dying
   */
  async shutdown()
  {
    this.log.info('shutting down GameHost for game %s', this.game.id);

    for (const {client, playerId} of this.clients) {
      this.dropClient(client, playerId);
    }
  }
}

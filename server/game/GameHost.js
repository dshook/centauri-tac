import loglevel from 'loglevel-decorator';

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
    this.log.info('client %s has connected for player %s', client.id, playerId);

    this.clients.push({client, playerId});
    client.once('close', () => this._clientClose(client));
  }

  /**
   * Manager is taking away a friend
   */
  dropClient(client, playerId)
  {
    this.log.info('client %s (player %s) is leaving', client.id, playerId);

    // TODO: probably more than this

    client.disconnect();
  }

  /**
   * A friend is leaving us
   */
  _clientClose(client)
  {
    this.log.info('client %s has disconnected', client.id);
    const index = this.clients.findIndex(x => x.client === client);
    this.clients.splice(index, 1);
  }

  /**
   * We are dying
   */
  async shutdown()
  {
    this.log.info('shutting down GameHost for game %s', this.game.id);
  }
}

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

    this.clients.push(client);
    client.once('close', () => this._clientClose(client));
  }

  /**
   * A friend is leaving us
   */
  _clientClose(client)
  {
    this.log.info('client %s has disconnected', client.id);
  }

  /**
   * We are dying
   */
  async shutdown()
  {
    this.log.info('shutting down GameHost for game %s', this.game.id);
  }
}

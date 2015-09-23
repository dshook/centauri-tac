import EventEmitter from 'events';

/**
 * Handle automatch stuff
 */
export default class Matchmaker extends EventEmitter
{
  constructor(netClient)
  {
    super();

    this.net = netClient;
    this.clients = new Set();
  }

  /**
   * Enter a player into the queue
   */
  async queuePlayer(playerId)
  {
    this.clients.add(playerId);

    this.log.info('player %s queueing, %s now in queue',
        playerId, this.queue.size);

    // see if player is already in a game
    this.net.sendCommand('gamelist', 'currentGameFor', playerId);

    const {game} = await this.net
      .recvCommand('currentGameFor', x => x.playerId === playerId);

    if (game) {
      this.log.info('player %s already in game %s', playerId, game.id);
      this.emit('game:current', {playerId, game});
    }

    this.log.info('TODO: add player to queue');
  }
}

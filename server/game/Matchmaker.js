import EventEmitter from 'events';
import loglevel from 'loglevel-decorator';

/**
 * Handle automatch stuff
 */
@loglevel
export default class Matchmaker extends EventEmitter
{
  constructor(netClient)
  {
    super();

    this.net = netClient;
    this.queue = new Set();
  }

  /**
   * Enter a player into the queue
   */
  async queuePlayer(playerId)
  {
    this.queue.add(playerId);

    this.log.info('player %s queueing, %s now in queue',
        playerId, this.queue.size);

    // see if player is already in a game
    await this.net.sendCommand('gamelist', 'currentGameFor', playerId);

    // const {game} = await this.net
    //   .recvCommand('currentGameFor', x => x.playerId === playerId);

    // if (game) {
    //   this.log.info('player %s already in game %s', playerId, game.id);
    //   this.emit('game:current', {playerId, game});
    // }

    this._emitStatus();
  }

  /**
   * Drop em
   */
  dequeuePlayer(playerId)
  {
    this.queue.delete(playerId);
    this.log.info('player %s dequeued, %s now in queue',
        playerId, this.queue.size);

    this._emitStatus();
  }

  /**
   * info son
   */
  _emitStatus()
  {
    this.emit('status', {
      queuedPlayers: this.queue.size,
    });
  }
}

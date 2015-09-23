import loglevel from 'loglevel-decorator';

/**
 * Handle automatch stuff
 */
@loglevel
export default class Matchmaker
{
  constructor(messenger, netClient)
  {
    this.messenger = messenger;
    this.net = netClient;
    this.queue = [];
  }

  /**
   * Enter a player into the queue
   */
  async queuePlayer(playerId)
  {
    if (this.queue.find(x => x === playerId)) {
      this.log.info('not adding player %s, already in the queue');
      return;
    }

    // see if player is already in a game
    await this.net.sendCommand('gamelist', 'getCurrentGame', playerId);

    this.queue.push(playerId);
    this.log.info('player %s queueing, %s now in queue',
        playerId, this.queue.length);

    this._emitStatus();

    this._processQueue();
  }

  /**
   * Drop em
   */
  dequeuePlayer(playerId)
  {
    const index = this.queue.findIndex(x => x === playerId);

    if (!~index) {
      this.log.info('player %s not in queue, not dequeueing', playerId);
      return;
    }

    this.queue.splice(index, 1);
    this.log.info('player %s dequeued, %s now in queue',
        playerId, this.queue.length);

    this._emitStatus();
  }

  /**
   * Check to see if we have a player
   */
  inQueue(playerId)
  {
    const index = this.queue.findIndex(x => x === playerId);
    return index !== -1;
  }

  /**
   * Let's match em
   */
  async _processQueue()
  {
    if (this.queue.length < 2) {
      return;
    }

    // immediately remove the players
    const pid1 = this.queue.shift();
    const pid2 = this.queue.shift();
    this._emitStatus();


    // boom
    const name = '(automatch)';
    const playerIds = [pid1, pid2];
    await this.net.sendCommand('gamelist', 'createFor', {name, playerIds});
  }

  /**
   * info son
   */
  _emitStatus()
  {
    this.messenger.emit('matchmaker:status', {
      queuedPlayers: this.queue.length,
    });
  }
}

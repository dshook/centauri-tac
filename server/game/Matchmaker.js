import loglevel from 'loglevel-decorator';
import _ from 'lodash';

const WAITING = 1;
const READY = 2;

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

    // see if player is already in a game -- until we know that they aren't
    // they're just "waiting"
    await this.net.sendCommand('gamelist', 'getCurrentGame', playerId);

    this.queue.push({playerId, status: WAITING});
    this.log.info('player %s waiting, %s in queue',
        playerId, this.queue.length);

    await this._emitStatus();

    await this._processQueue();
  }

  /**
   * Drop em
   */
  async dequeuePlayer(playerId)
  {
    const index = this.queue.findIndex(x => x.playerId === playerId);

    if (!~index) {
      return;
    }

    this.queue.splice(index, 1);
    this.log.info('player %s dequeued, %s now in queue',
        playerId, this.queue.length);

    await this._emitStatus();
  }

  /**
   * Check to see if we have a player
   */
  inQueue(playerId)
  {
    const index = this.queue.findIndex(x => x.playerId === playerId);
    return index !== -1;
  }

  /**
   * Allowed
   */
  async confirmQueue(playerId)
  {
    const entry = this.queue.find(x => x.playerId === playerId);

    if (!entry) {
      return;
    }

    this.log.info('player %s confirmed for queue', playerId);
    entry.status = READY;
    await this._processQueue();
  }

  /**
   * Let's match em
   */
  async _processQueue()
  {
    const ready = this.queue.filter(x => x.status === READY);

    if (ready.length < 2) {
      this.log.info('not enough ready players to process queue yet');
      return;
    }

    const [pid1, pid2] = ready.map(x => x.playerId);
    const playerIds = [pid1, pid2];

    // remove from original queue
    _.remove(this.queue, x => playerIds.some(id => id === x.playerId));
    this.log.info('removed %s from queue, now %s left',
        playerIds.length, this.queue.length);
    await this._emitStatus();

    // boom
    const name = '(automatch)';

    this.log.info('match found! creating game for %s', playerIds.join(','));

    await this.net.sendCommand('gamelist', 'createFor', {name, playerIds});
  }

  /**
   * info son
   */
  async _emitStatus()
  {
    await this.messenger.emit('matchmaker:status', {
      queuedPlayers: this.queue.length,
    });
  }
}

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
  constructor(emitter, gamelistManager)
  {
    this.emitter = emitter;
    this.gamelistManager = gamelistManager;

    setInterval(() => this._processQueue(), 500);

    this.queue = [];
  }

  /**
   * Enter a player into the queue
   */
  async queuePlayer(playerId, client)
  {
    if (this.queue.find(x => x === playerId)) {
      this.log.info('not adding player %s, already in the queue');
      return;
    }

    // add as waiting status
    const entry = {playerId, status: WAITING};
    this.queue.push(entry);
    this.log.info('player %s waiting, %s in queue', playerId, this.queue.length);
    await this._emitStatus(playerId, true, false);

    const game = await this.gamelistManager.getCurrentGame(playerId);

    // if the player's already in a game, send it on but them boot em put
    if (!game) {
      entry.status = READY;
    }
    else {
      this.log.info('player %s already in game %s, removing from queue', playerId, game.id);
      await this.dequeuePlayer(playerId, client);
      await this._emitStatus(playerId, false, true);
      await this.emitter.emit('game:current', {game, playerId});
    }
  }

  /**
   * Drop em
   */
  async dequeuePlayer(playerId, client)
  {
    const index = this.queue.findIndex(x => x.playerId === playerId);

    if (!~index) {
      return;
    }

    this.queue.splice(index, 1);
    this.log.info('player %s dropped, %s in queue', playerId, this.queue.length);
    await this._emitStatus(playerId, false, false);
  }

  /**
   * Let's match em
   */
  async _processQueue()
  {
    if (this.queue.length < 2) {
      return;
    }

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

    await this._emitStatus(pid1, false, true);
    await this._emitStatus(pid2, false, true);

    // boom
    const name = '(automatch)';
    this.log.info('match found! creating game for %s', playerIds.join(','));
    await this.emitter.emit('gamelist:createFor', {name, playerIds});
  }

  /**
   * info son
   */
  async _emitStatus(playerId, inQueue, beingMatched)
  {
    await this.emitter.emit('matchmaker:status', {
      playerId, inQueue, beingMatched
    });
  }
}

import loglevel from 'loglevel-decorator';
import _ from 'lodash';
import AiPlayer from './ctac/ai/AiPlayer';

const WAITING = 1;
const READY = 2;

/**
 * Handle automatch stuff
 */
@loglevel
export default class Matchmaker
{
  constructor(emitter, gameManager, componentsConfig, auth)
  {
    this.emitter = emitter;
    this.gameManager = gameManager;
    this.componentsConfig = componentsConfig;
    this.auth = auth;

    setInterval(() => this._processQueue(), 500);

    this.queue = [];
  }

  /**
   * Enter a player into the queue
   */
  async queuePlayer(playerId, deckId)
  {
    this.log.info('Trying to find player %s in queue %j', playerId, this.queue);
    if (this.queue.find(x => x.playerId === playerId)) {
      this.log.info('not adding player %s, already in the queue', playerId);
      return;
    }

    // add as waiting status
    const entry = {playerId, deckId, status: WAITING};
    this.queue.push(entry);
    this.log.info('player %s waiting, %s in queue', playerId, this.queue.length);
    await this._emitStatus(playerId, true, false);

    const game = await this.gameManager.getCurrentGame(playerId);

    // if the player's already in a game, send it on but them boot em put
    if (!game) {
      entry.status = READY;
    }
    else {
      this.log.info('player %s already in game %s, removing from queue', playerId, game.id);
      await this.dequeuePlayer(playerId);
      await this._emitStatus(playerId, false, true);
      await this.emitter.emit('game:current', {game, playerId});
    }
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
    this.log.info('player %s dropped, %s in queue', playerId, this.queue.length);
    await this._emitStatus(playerId, false, false);
  }

  /**
   * Let's match em
   */
  async _processQueue()
  {
    if(this.componentsConfig.dev && this.queue.length === 1 && this.queue[0].playerId !== -1){
      //Create AI and use deck in DB for them to use
      this.log.info('Adding AI player for match');
      let ai = new AiPlayer(-1, this.auth);
      await this.emitter.emit('ai:add', ai.client);
      await this.queuePlayer(ai.id, -1);
    }

    const ready = this.queue.filter(x => x.status === READY);
    if (this.queue.length < 2 || ready.length < 2) {
      //this.log.info('not enough ready players to process queue yet');
      return;
    }


    const matchedPlayers = ready.slice(0, 2);
    const playerIds = matchedPlayers.map(m => m.playerId);
    this.log.info('Matching players %j ', playerIds);

    // remove from original queue
    _.remove(this.queue, x => playerIds.some(id => id === x.playerId));
    this.log.info('removed %s from queue, now %s left', playerIds.length, this.queue.length);

    for(let playerId of playerIds){
      await this._emitStatus(playerId, false, true);
    }

    // boom
    const name = '';
    this.log.info('match found! creating game for %s', playerIds.join(','));
    await this.emitter.emit('gamelist:createFor', {name, matchedPlayers});
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

import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';
import MovePiece from '../actions/MovePiece.js';

/**
 * Deals with handling turn stuff and processing the action queue. "low level"
 * game actions
 */
@loglevel
export default class GameController
{
  constructor(players, queue)
  {
    this.players = players;
    this.queue = queue;
  }

  /**
   * Catch a client up if they need actions
   */
  @on('playerCommand', x => x === 'getActionsSince')
  async getActionsSince(command, actionId, player)
  {
    this.log.info('player %s catching up on actions since %s',
        player.id, actionId);

    const actions = this.queue.iterateCompletedSince(actionId);

    player.client.send('actionHistoryStart');

    for (const action of actions) {
      this._sendAction(player, action);
    }

    player.client.send('actionHistoryComplete');
  }

  /**
   * Pass turn to another player
   */
  @on('playerCommand', x => x === 'endTurn')
  passTurn(command, toPlayerId = null, player)
  {
    let id = toPlayerId;

    // implicit just pass to the other player
    if (toPlayerId === null) {
      if (this.players.length !== 2) {
        throw new Error('to field required when not 2 players');
      }

      const toPlayer = this.players.find(x => x.id !== player.id);
      id = toPlayer.id;
    }

    const action = new PassTurn(id, player.id);
    this.queue.push(action);
    this.queue.processUntilDone();
  }

  /**
   * Move a piece 
   */
  @on('playerCommand', x => x === 'move')
  movePiece(command, data)
  {
    let {pieceId, route} = data;

    if(route){
      for (let step of route) {
        const action = new MovePiece(pieceId, step);
        this.queue.push(action);
      }
      this.queue.processUntilDone();
    }
  }

  /**
   * An action has finished
   */
  @on('actionComplete')
  async actionComplete(action)
  {
    for (const player of this.players) {

      // disconnceted played
      if (!player.client) {
        continue;
      }

      this._sendAction(player, action);
    }
  }

  /**
   * When queue processing is complete, tell all clients
   */
  @on('qpc')
  queueProcessComplete(ticks)
  {
    for (const p of this.players.filter(x => x.client)) {
      p.client.send('qpc', ticks);
    }
  }

  /**
   * When queue proc is started, tell all clients
   */
  @on('qps')
  queueProcessStart(ticks)
  {
    for (const p of this.players.filter(x => x.client)) {
      p.client.send('qps', ticks);
    }
  }

  /**
   * Broadcast an action
   */
  _sendAction(player, action)
  {
    player.client.send('action:' + action.constructor.name, action);
  }
}

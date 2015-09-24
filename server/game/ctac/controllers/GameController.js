import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';

/**
 * Deals with handling turn stuff and processing the action queue
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
   * Send current state of the queue to the players when the game starts
   * (basically the first turn and anyhting else inited)
   */
  async start()
  {
    for (const player of this.players.filter(x => x.client)) {
      for (const action of this.queue.iterateCompletedSince()) {
        this._sendAction(player, action);
      }
    }
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

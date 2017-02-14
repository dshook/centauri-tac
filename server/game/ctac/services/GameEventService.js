import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';

/**
 * Stuff that happens on a regular basis for the game
 */
@loglevel
export default class GameEventService
{
  constructor(app, queue)
  {
    this.turnTimer = 10000;
    this.queue = queue;
    setInterval(() => this.passTurn, this.turnTimer);
  }

  passTurn(){
    this.log.info('Passing turn');
    const action = new PassTurn();
    this.queue.push(action);
    this.queue.processUntilDone();
  }
}

import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';
import IntervalTimer from 'interval-timer';

/**
 * Stuff that happens on a regular basis for the game
 */
@loglevel
export default class GameEventService
{
  constructor(app, queue)
  {
    this.queue = queue;
    this.autoTurnInterval = new IntervalTimer('Auto Turn Interval', () => this.passTurn(), 10000);

    //app.registerInstance('autoTurnInterval', this.autoTurnInterval);
    app.registerInstance('gameEventService', this);
  }

  shutdown()
  {
    this.log.info('Killing Game Event Timers');
    this.autoTurnInterval.stop();
  }

  passTurn(){
    this.log.info('Auto Passing turn');
    const action = new PassTurn();
    this.queue.push(action);
    this.queue.processUntilDone();
  }
}


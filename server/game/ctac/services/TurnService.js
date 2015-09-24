import TurnState from '../models/TurnState.js';
import loglevel from 'loglevel-decorator';
import TurnProcessor from '../processors/TurnProcessor.js';

/**
 * Expose the turn and start / stop the timers
 */
@loglevel
export default class TurnService
{
  constructor(app, queue)
  {
    this.state = new TurnState();
    app.registerInstance('turnState', this.state);
    queue.addProcessor(TurnProcessor);
  }
}

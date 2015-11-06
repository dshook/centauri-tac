import TurnState from '../models/TurnState.js';
import PlayerResourceState from '../models/PlayerResourceState.js';
import loglevel from 'loglevel-decorator';
import TurnProcessor from '../processors/TurnProcessor.js';
import PlayerResourceProcessor from '../processors/PlayerResourceProcessor.js';

/**
 * Expose the turn model and add the processor to the action pipeline
 */
@loglevel
export default class TurnService
{
  constructor(app, queue)
  {
    this.state = new TurnState();
    this.playerResourceState = new PlayerResourceState();
    app.registerInstance('turnState', this.state);
    app.registerInstance('playerResourceState', this.playerResourceState);
    queue.addProcessor(TurnProcessor);
    queue.addProcessor(PlayerResourceProcessor);
  }
}

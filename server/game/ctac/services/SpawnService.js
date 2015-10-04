import PieceState from '../models/PieceState.js';
import loglevel from 'loglevel-decorator';
import SpawnProcessor from '../processors/SpawnProcessor.js';

/**
 * Expose the turn model and add the processor to the action pipeline
 */
@loglevel
export default class SpawnService
{
  constructor(app, queue)
  {
    this.state = new PieceState();
    app.registerInstance('pieceState', this.state);
    queue.addProcessor(SpawnProcessor);
  }
}

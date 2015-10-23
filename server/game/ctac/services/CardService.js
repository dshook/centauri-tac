import loglevel from 'loglevel-decorator';
import ActivateCardProcessor from '../processors/ActivateCardProcessor.js';

/**
 * Expose the turn model and add the processor to the action pipeline
 */
@loglevel
export default class CardService
{
  constructor(app, queue)
  {
    queue.addProcessor(ActivateCardProcessor);
  }
}

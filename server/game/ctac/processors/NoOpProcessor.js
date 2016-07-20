import Message from '../actions/Message.js';
import loglevel from 'loglevel-decorator';

/**
 * Processor for all the actions that only need clientside processing
 */
@loglevel
export default class NoOpProcessor
{
  async handleAction(action, queue)
  {
    if ((action instanceof Message)) {
      queue.complete(action);
    }
  }
}

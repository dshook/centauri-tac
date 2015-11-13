import Message from '../actions/Message.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Processor for all the actions that only need clientside processing
 */
@loglevel
export default class NoOpProcessor
{
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof Message)) {
      return;
    }

    queue.complete(action);
  }
}

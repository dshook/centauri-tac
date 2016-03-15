import Message from '../actions/Message.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

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

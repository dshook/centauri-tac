import Message from '../actions/Message.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
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
    if (
      (action instanceof Message)
      || (action instanceof PieceStatusChange)
    ) {
      queue.complete(action);
    }
  }
}

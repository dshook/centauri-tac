import SetPlayerResource from '../actions/SetPlayerResource.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle playing a card
 */
@loglevel
export default class PlayerResourceProcessor
{
  constructor(playerResourceState)
  {
    this.playerResourceState = playerResourceState;
  }
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof SetPlayerResource)) {
      return;
    }
    var newTotal = this.playerResourceState.adjust(action.playerId, action.change);
    action.newTotal = newTotal;

    queue.complete(action);
    this.log.info('player %s used %s resources', action.playerId, action.amount);
  }
}

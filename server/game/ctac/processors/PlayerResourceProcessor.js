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
    this.playerResourceState.expend(action.playerId, action.amount);

    queue.complete(action);
    this.log.info('player %s used %s resources', action.playerId, action.amount);
  }
}

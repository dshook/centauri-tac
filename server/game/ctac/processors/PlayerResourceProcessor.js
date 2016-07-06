import SetPlayerResource from '../actions/SetPlayerResource.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle getting temporary or permanent energy
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
    if(action.permanent){
      this.playerResourceState.incriment(action.playerId, action.change);
    }
    if(action.filled){
      this.playerResourceState.adjust(action.playerId, action.change);
    }

    action.newAmount = this.playerResourceState.get(action.playerId);
    action.newMax = this.playerResourceState.getMax(action.playerId);

    queue.complete(action);
    this.log.info('player %s used %s resources, now at %s',
      action.playerId, action.change, action.newAmount);
  }
}

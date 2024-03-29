import ChargeUp from '../actions/ChargeUp.js';
import loglevel from 'loglevel-decorator';

@loglevel
export default class ChargeUpProcessor
{
  constructor(playerResourceState, cardEvaluator)
  {
    this.playerResourceState = playerResourceState;
    this.cardEvaluator = cardEvaluator;
  }
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof ChargeUp)) {
      return;
    }

    this.playerResourceState.charges[action.playerId] += action.change;
    action.charge = this.playerResourceState.charges[action.playerId];

    this.cardEvaluator.evaluatePlayerEvent('chargeChange', action.playerId);

    queue.complete(action);
    this.log.info('player %s got %s charge, now at %s',
      action.playerId, action.change, action.charge);
  }
}

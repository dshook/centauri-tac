import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import ActivateCard from '../actions/ActivateCard.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle playing a card
 */
@loglevel
export default class ActivateCardProcessor
{
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof ActivateCard)) {
      return;
    }

    queue.push(new SpawnPiece(action.playerId, action.cardId, action.position));

    queue.complete(action);
    this.log.info('player %s played card %s at %s',
      action.playerId, action.cardId, action.position);
  }
}

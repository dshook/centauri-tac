import PieceArmorChange from '../actions/PieceArmorChange.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle pieces losing or gaining armor
 */
@loglevel
export default class PieceArmorChangeProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceArmorChange)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to change armor on for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(action.change == 0){
      this.log.warn('No armor to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    if(action.change <= 0 && action.change < piece.armor){
      this.log.warn("Can't remove more armor than piece %s has. Use piece health change instead", action.pieceId);
      return queue.cancel(action);
    }

    piece.armor = piece.armor + action.change;
    action.newArmor = piece.armor;

    this.log.info('piece %s %s %s armor, now %s',
      action.pieceId, (action.change > 0 ? 'gained' : 'lost'), action.change, action.newArmor);
    queue.complete(action);
  }
}

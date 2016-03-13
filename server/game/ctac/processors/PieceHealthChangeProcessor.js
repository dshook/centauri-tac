import GamePiece from '../models/GamePiece.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import Statuses from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle pieces losing or gaining their current health
 */
@loglevel
export default class PieceHealthChangeProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceHealthChange)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to change health on for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(action.change == 0){
      this.log.warn('No health to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    let hpBeforeChange = piece.health;

    //check for shield and nullify damage
    if(piece.statuses.includes(Statuses.Shield)){
      action.change = 0;
      action.bonus = 0;
      queue.push(new PieceStatusChange(piece.id, null, Statuses.Shield));
    }

    piece.health = piece.health + action.change + (action.bonus || 0);

    //cap hp at base health and adjust action change amounts
    if(piece.health > piece.baseHealth){
      action.change = piece.baseHealth - hpBeforeChange;
      piece.health = piece.baseHealth;
    }
    action.newCurrentHealth = piece.health;

    if(action.change < 0){
      this.cardEvaluator.evaluatePieceEvent('damaged', piece);
    }

    if(piece.health <= 0){
      this.cardEvaluator.evaluatePieceEvent('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('piece %s %s %s health',
      action.pieceId, (action.change > 0 ? 'gained' : 'lost'), action.change);
    queue.complete(action);
  }
}

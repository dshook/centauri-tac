import GamePiece from '../models/GamePiece.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle pieces losing or gaining their current health
 */
@loglevel
export default class HealthChangeProcessor
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
      this.log.info('Cannot find piece to change health on for id %s', action.pieceId);
      queue.cancel(action);
      return;
    }
    if(action.change == 0){
      this.log.info('No health to change for piece %s', action.pieceId);
      queue.cancel(action);
      return;
    }

    action.newCurrentHealth = piece.health + action.change;

    piece.health = action.newCurrentHealth;

    if(piece.health <= 0){
      this.cardEvaluator.evaluateAction('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('piece %s %s %s health',
      action.pieceId, (action.change > 0 ? 'gained' : 'lost'), action.change);
    queue.complete(action);
  }
}

import PieceDestroyed from '../actions/PieceDestroyed.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle pieces getting k.o'd
 */
@loglevel
export default class PieceDestroyedProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceDestroyed)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to change health on for id %s', action.pieceId);
      return queue.cancel(action);
    }

    this.cardEvaluator.evaluatePieceEvent('death', piece);
    this.pieceState.remove(piece.id);

    this.log.info('piece %s destroyed', action.pieceId);
    queue.complete(action);
  }
}

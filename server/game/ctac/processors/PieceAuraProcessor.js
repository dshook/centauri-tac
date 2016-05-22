import GamePiece from '../models/GamePiece.js';
import PieceAura from '../actions/PieceAura.js';
import loglevel from 'loglevel-decorator';

/**
 * Attach an aura to a piece
 */
@loglevel
export default class PieceAuraProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceAura)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to add aura to for id %s', action.pieceId);
      return queue.cancel(action);
    }

    piece.auras.push(action);

    this.log.info('piece %s got aura %s', action.pieceId, action.name);
    queue.complete(action);
  }
}

import GamePiece from '../models/GamePiece.js';
import PieceAura from '../actions/PieceAura.js';
import loglevel from 'loglevel-decorator';

/**
 * Attach an aura to a piece, update aura processor takes care of the rest
 */
@loglevel
export default class PieceAuraProcessor
{
  constructor(pieceState)
  {
    this.pieceState = pieceState;
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

    if(piece.aura){
      this.log.warn('Replacing aura %j on piece %s', piece.aura, action.pieceId);
    }
    piece.aura = action;

    this.log.info('piece %s got aura %s', action.pieceId, action.name);
    queue.complete(action);
  }
}

import GamePiece from '../models/GamePiece.js';
import Direction from '../models/Direction.js';
import RotatePiece from '../actions/RotatePiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class RotatePieceProcessor
{
  constructor(pieceState)
  {
    this.pieceState = pieceState;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof RotatePiece)) {
      return;
    }

    var piece = this.pieceState.piece(action.pieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to rotate %j', action.pieceId, this.pieceState);
      return queue.cancel(action);
    }

    piece.direction = Direction[action.direction];

    queue.complete(action);
    this.log.info('rotated piece %s to %s',
      action.pieceId, action.direction);
  }
}

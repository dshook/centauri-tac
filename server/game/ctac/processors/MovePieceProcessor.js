import GamePiece from '../models/GamePiece.js';
import MovePiece from '../actions/MovePiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class MovePieceProcessor
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
    if (!(action instanceof MovePiece)) {
      return;
    }

    //TODO: validate against board state and all that jazz
    var piece = this.pieceState.pieces.filter(x => x.id == action.pieceId)[0];
    if(!piece){
      this.log.info('Could not find piece %s to move %j', action.pieceId, this.pieceState);
      queue.cancel(action);
      return;
    }

    var currentPosition = piece.position;
    piece.position = action.to;

    queue.complete(action);
    this.log.info('moved piece %s from %s to %s',
      action.pieceId, currentPosition, action.to);
  }
}

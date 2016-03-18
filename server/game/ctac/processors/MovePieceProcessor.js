import GamePiece from '../models/GamePiece.js';
import Direction from '../models/Direction.js';
import {faceDirection} from '../models/Direction.js';
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

    var piece = this.pieceState.piece(action.pieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to move %j', action.pieceId, this.pieceState);
      return queue.cancel(action);
    }

    //check to make sure that the piece isn't moving on top of another piece
    //unless it's a friendly piece and not the final destination
    let occupyingPieces = this.pieceState.pieces.filter(p => p.position.equals(action.to));
    if(occupyingPieces.length > 0){
      if(occupyingPieces.length > 1) throw 'Multiple pieces already occupying position';
      let otherPiece = occupyingPieces[0];
      //peek two because the current action hasn't been completed yet
      let upcomingQueue = queue.peek(2);
      let nextActionIsNotMove = upcomingQueue.length < 1 || !(upcomingQueue[1] instanceof MovePiece);
      if(piece.id != otherPiece.id && (otherPiece.playerId != piece.playerId || nextActionIsNotMove)){
        this.log.warn('Cannot move piece %j on top of %j', piece, otherPiece);
        return queue.cancel(action);
      }
    }

    //determine direction piece should be facing to see if rotation is necessary
    let targetDirection = faceDirection(action.to, piece.position);
    action.direction = targetDirection;
    var currentPosition = piece.position;
    piece.position = action.to;

    queue.complete(action);
    this.log.info('moved piece %s from %s to %s',
      action.pieceId, currentPosition, action.to);
  }
}

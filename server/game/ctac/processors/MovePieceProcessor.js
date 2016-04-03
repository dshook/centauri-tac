import GamePiece from '../models/GamePiece.js';
import Direction from '../models/Direction.js';
import Statuses from '../models/Statuses.js';
import {faceDirection} from '../models/Direction.js';
import MovePiece from '../actions/MovePiece.js';
import AttackPiece from '../actions/AttackPiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class MovePieceProcessor
{
  constructor(pieceState, mapState)
  {
    this.pieceState = pieceState;
    this.mapState = mapState;
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

    if((piece.statuses & Statuses.Paralyze) || (piece.statuses & Statuses.Root)){
      this.log.warn('Cannot move piece %s with status %s', piece.id, piece.statuses);
      return queue.cancel(action);
    }

    //determine direction piece should be facing to see if rotation is necessary
    let targetDirection = faceDirection(action.to, piece.position);
    action.direction = targetDirection;
    var currentPosition = piece.position;
    piece.position = action.to;

    queue.complete(action);
    this.log.info('moved piece %s from %s to %s',
      action.pieceId, currentPosition, action.to);

    //figure out if we've stepped into an enemy taunted area
    //we do this by finding all the enemy taunt pieces, getting the combined area they block off
    //and then seeing if we stepped into it
    let tauntPieces = this.pieceState.withStatus(Statuses.Taunt)
      .filter(p => p.playerId != piece.playerId);
    for(let tauntPiece of tauntPieces){
      let tauntPositions = this.mapState.getKingTilesInRadius(tauntPiece.position, 1);
      if(tauntPositions.length > 0 && tauntPositions.find(p => p.tileEquals(piece.position) )){
        this.log.info('Unit %j stepped onto a taunt area of %j', piece, tauntPiece);
        //cancel any upcoming move actions
        let upcomingQueue = queue.peek();
        for(let upcomingAction of upcomingQueue){
          if((upcomingAction instanceof MovePiece) && upcomingAction.pieceId == piece.id){
            queue.cancel(upcomingAction);
          }else{
            break;
          }
        }

        //add implicit attack action
        queue.push(new AttackPiece(piece.id, tauntPiece.id, true));
      }
    }
  }
}

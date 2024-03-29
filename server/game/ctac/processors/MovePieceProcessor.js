import Statuses from '../models/Statuses.js';
import {faceDirection} from '../models/Direction.js';
import MovePiece from '../actions/MovePiece.js';
import UpdateAura from '../actions/UpdateAura.js';
import AttackPiece from '../actions/AttackPiece.js';
import Message from '../actions/Message.js';
import loglevel from 'loglevel-decorator';

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
    //or if it's a teleport swip swap
    let occupyingPieces = this.pieceState.pieces.filter(p => p.position.tileEquals(action.to));
    if(occupyingPieces.length > 0){
      if(occupyingPieces.length > 1){
        this.log.warn('Cannot move piece %j on top of %j', piece, occupyingPieces[0]);
        return queue.cancel(action);
      }

      let otherPiece = occupyingPieces[0];
      //peek two because the current action hasn't been completed yet
      let upcomingQueue = queue.peek(2);
      let nextAction = upcomingQueue[1];
      let nextActionIsNotMove = upcomingQueue.length < 1 || !(nextAction instanceof MovePiece);
      let isMovingOnTopOfEnemy = piece.id != otherPiece.id && (otherPiece.playerId != piece.playerId || nextActionIsNotMove);
      let isTeleportSwap = (nextAction instanceof MovePiece)
        && nextAction.pieceId === otherPiece.id
        && nextAction.to.tileEquals(piece.position)
        && action.isTeleport
        && nextAction.isTeleport;
      if(isMovingOnTopOfEnemy && !isTeleportSwap && !action.ignoreCollisionCheck){
        this.log.warn('Cannot move piece %j on top of %j', piece, otherPiece);
        return queue.cancel(action);
      }
    }
    let isFlying = !!(piece.statuses & Statuses.Flying);
    if(!action.isJump && (piece.statuses & Statuses.Petrify) || (piece.statuses & Statuses.Root)){
      this.log.warn('Cannot move piece %s with status %s', piece.id, piece.statuses);
      return queue.cancel(action);
    }

    let currentTile = this.mapState.getTile(piece.position);
    let destinationTile = this.mapState.getTile(action.to)

    if(!destinationTile || destinationTile.unpassable){
      this.log.warn('Cannot move piece %s to unpassable tile %s'
        , piece.id, destinationTile ? destinationTile.position : 'Missing dest: ' + action.to);
      queue.cancel(action);
      this.cancelUpcomingMoves(queue, piece);
      queue.push(new Message("That tile doesn't look safe!", piece.playerId));
      return;
    }

    //check height differential
    if(!action.isJump && !isFlying ){
      if(!this.mapState.isHeightPassable(currentTile, destinationTile)){
        this.log.warn('Cannot move piece %j up height diff', piece);
        return queue.cancel(action);
      }
    }

    let travelDistance = this.mapState.tileDistance(currentTile.position, destinationTile.position);
    if(!action.isJump && !isFlying && travelDistance > 1){
      this.log.warn('Cannot move piece %j more than 1 at a time', piece);
      return queue.cancel(action);
    }

    if(!action.isJump && piece.moveCount >= piece.movement){
      this.log.warn('Piece %j has already exceeded move count', piece);
      return queue.cancel(action);
    }

    //determine direction piece should be facing to see if rotation is necessary
    let targetDirection = faceDirection(action.to, piece.position);
    action.direction = targetDirection;
    piece.position = action.to;
    piece.direction = action.direction;

    //Jumps only counts as a single move but everything else is based on distance which should be 1
    //for non flying moves
    if(isFlying){
      action.isJump = true;
      piece.moveCount += travelDistance;
    }
    else if (action.isJump)
    {
      piece.moveCount++;
    }
    else
    {
      piece.moveCount += travelDistance;
    }

    //ranged pieces can't move and attack on the same turn
    if(piece.range != null){
      piece.attackCount++;
    }

    queue.complete(action);
    this.log.info('moved piece %s from %s to %s, direction %s moveCount %s',
      action.pieceId, currentTile.position, action.to, action.direction, piece.moveCount);

    //figure out if we've stepped into an enemy taunted area
    //we do this by finding all the enemy taunt pieces, getting the combined area they block off
    //and then seeing if we stepped into it

    //if we're moving through a piece, don't consider a taunt action
    if(occupyingPieces.length > 0) return;

    let tauntPieces = this.pieceState.withStatus(Statuses.Taunt)
      .filter(p => p.playerId != piece.playerId);
    for(let tauntPiece of tauntPieces){
      let tauntPositions = this.mapState.getKingTilesInRadius(tauntPiece.position, 1);
      if(tauntPositions.length > 0 &&
          tauntPositions.find(p => p.tileEquals(piece.position)
          )
        )
      {
        //additional game logic shiet to make sure it's a valid taunt attack
        if(
            !this.mapState.isHeightPassable(this.mapState.getTile(tauntPiece.position), destinationTile)
            || (tauntPiece.statuses & Statuses.Cloak)
        ){
          continue;
        }

        this.log.info('Piece %s stepped onto a taunt area of %s', piece.id, tauntPiece.id);
        //cancel any upcoming move actions so we don't move past the taunt
        this.cancelUpcomingMoves(queue, piece);

        //add implicit attack action
        queue.push(new AttackPiece(piece.id, tauntPiece.id, true));
        break;
      }
    }

    queue.pushFront(new UpdateAura());
  }

  cancelUpcomingMoves(queue, piece){
    piece.moveCount = 99;
    let upcomingQueue = queue.peek();
    this.log.info('Queue Peek %j', upcomingQueue);
    for(let upcomingAction of upcomingQueue){
      if((upcomingAction instanceof MovePiece) && upcomingAction.pieceId == piece.id){
        this.log.info('Cancelling move');
        queue.cancel(upcomingAction);
      }else if((upcomingAction instanceof AttackPiece) && upcomingAction.attackingPieceId == piece.id){
        this.log.info('Cancelling attack');
        queue.cancel(upcomingAction);
      }else{
        break;
      }
    }
  }
}

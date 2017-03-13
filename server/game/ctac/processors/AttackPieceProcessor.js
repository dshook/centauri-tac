import Statuses from '../models/Statuses.js';
import AttackPiece from '../actions/AttackPiece.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import {directionOf, faceDirection} from '../models/Direction.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the units attacking each other
 */
@loglevel
export default class AttackPieceProcessor
{
  constructor(pieceState, cardEvaluator, mapState, turnState)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.mapState = mapState;
    this.turnState = turnState;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof AttackPiece)) {
      return;
    }

    var attacker = this.pieceState.piece(action.attackingPieceId);
    var target = this.pieceState.piece(action.targetPieceId);

    if(!attacker || !target ){
      this.log.warn('Attacker or target not found in attack %j', this.pieceState);
      return queue.cancel(action);
    }

    if(attacker.id === target.id){
      this.log.warn('Stop hitting yourself!');
      return queue.cancel(action);
    }

    let targetDistance = Number.MAX_VALUE;
    if(action.isTauntAttack || attacker.range != null){
      targetDistance = this.mapState.kingDistance(attacker.position, target.position);
    }else{
      targetDistance = this.mapState.tileDistance(attacker.position, target.position);
    }
    let rangedAttack = attacker.range != null && targetDistance > 1;
    if(targetDistance > 1 && (attacker.range != null && attacker.range < targetDistance)){
      this.log.warn('Attacker too far away from target %s %s', targetDistance, attacker.range);
      return queue.cancel(action);
    }

    //check height differential
    let attackerTile = this.mapState.getTile(attacker.position);
    let targetTile = this.mapState.getTile(target.position)
    if(!this.mapState.isHeightPassable(attackerTile, targetTile)){
        this.log.info('Cannot attack due to tile heigh diff');
        return queue.cancel(action);
    }

    if(!action.isTauntAttack && (attacker.statuses & Statuses.Paralyze || attacker.statuses & Statuses.CantAttack)){
      this.log.warn('Cannot attack with piece %s with status %s', attacker.id, attacker.statuses);
      return queue.cancel(action);
    }

    if(target.statuses & Statuses.Cloak){
      this.log.warn('Cannot attack piece %s with Cloak', target.id);
      return queue.cancel(action);
    }

    //check if piece is 'old' enough to attack
    if(!attacker.bornOn ||
      ((this.turnState.currentTurn - attacker.bornOn) < 1 && !(attacker.statuses & Statuses.Charge))
    ){
      this.log.warn('Piece %s must wait a turn to attack', attacker.id);
      return queue.cancel(action);
    }

    let maxAttacks = (attacker.statuses & Statuses.DyadStrike) ? 2 : 1;
    if(attacker.attackCount >= maxAttacks){
      this.log.warn('Piece %s has already attacked', attacker.id);
      return queue.cancel(action);
    }

    //determine direction piece should be facing to see if rotation is necessary
    let attackerNewDirection = faceDirection(target.position, attacker.position);
    attacker.direction = action.direction = attackerNewDirection;

    let targetNewDirection = faceDirection(attacker.position, target.position);
    target.direction = action.targetDirection = targetNewDirection;

    let bonus = 0;
    let bonusMsg = null;
    // let facingDirection = directionOf(attackerNewDirection, target.direction);
    // if(facingDirection == 'behind' && !rangedAttack){
    //   this.log.info('Backstab triggered, attackerDirection: %s attackerNewDirection: %s, target.direction: %s'
    //     , action.direction, attackerNewDirection, target.direction);
    //   bonus = -1;
    //   bonusMsg = 'Backstab';
    // }

    //do double checks for paralyze and can't attack here if it's a taunt attack
    if(!action.isTauntAttack || !(attacker.statuses & Statuses.CantAttack || attacker.statuses & Statuses.Paralyze)){
      queue.push(new PieceHealthChange(action.targetPieceId, -attacker.attack, bonus, bonusMsg));
    }

    //counter attack if in range
    if(!rangedAttack || (target.range != null && target.range >= targetDistance)){
      if(!action.isTauntAttack || !(target.statuses & Statuses.CantAttack || target.statuses & Statuses.Paralyze)){
        queue.push(new PieceHealthChange(action.attackingPieceId, -target.attack));
      }
    }

    attacker.attackCount++;
    if(attacker.range != null){
      attacker.hasMoved = true;
    }

    //Remove cloak once they've attacked
    if(attacker.statuses & Statuses.Cloak){
      queue.push(new PieceStatusChange(attacker.id, null, Statuses.Cloak ));
    }

    this.cardEvaluator.evaluatePieceEvent('attacks', attacker);

    this.log.info('piece %s (%s/%s) attacked %s (%s/%s) direction %s',
      attacker.id, attacker.attack, attacker.health,
      target.id, target.attack, target.health, attackerNewDirection);
    queue.complete(action);
  }
}

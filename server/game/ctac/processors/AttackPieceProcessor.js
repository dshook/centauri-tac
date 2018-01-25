import Statuses from '../models/Statuses.js';
import AttackPiece from '../actions/AttackPiece.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import TilesCleared from '../actions/TilesCleared.js';
import loglevel from 'loglevel-decorator';
import Message from '../actions/Message.js';
import Constants from '../util/Constants.js';
import {faceDirection, cleavePositions, piercePositions} from '../models/Direction.js';

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
    if(targetDistance > 1 && (attacker.range == null || attacker.range < targetDistance)){
      this.log.warn('Attacker too far away from target %s %s', targetDistance, attacker.range);
      //queue.push(new Message('Too far away!', action.playerId));
      return queue.cancel(action);
    }

    //check height differential
    let attackerTile = this.mapState.getTile(attacker.position);
    let targetTile = this.mapState.getTile(target.position)
    if(attacker.range == null && !this.mapState.isHeightPassable(attackerTile, targetTile)){
        this.log.info('Cannot attack due to tile heigh diff');
        queue.push(new Message('Can\'t attack up that slope!', action.playerId));
        return queue.cancel(action);
    }

    if(!action.isTauntAttack && (attacker.statuses & Statuses.CantAttack)){
      this.log.warn('Cannot attack with piece %s with Cant Attack', attacker.id, attacker.statuses);
      queue.push(new Message('Unable to attack with this status!', action.playerId));
      return queue.cancel(action);
    }

    if(attacker.statuses & Statuses.Paralyze){
      this.log.info('Cannot attack with piece %s when paralyzed', attacker.id);
      return queue.cancel(action);
    }

    if(!action.isTauntAttack && (target.statuses & Statuses.Cloak)){
      this.log.warn('Cannot attack piece %s with Cloak', target.id);
      queue.push(new Message('Cannot attack a cloaked minion!', action.playerId));
      return queue.cancel(action);
    }

    //check if piece is 'old' enough to attack
    if(!action.isTauntAttack &&
      (
        attacker.bornOn === null ||
        ((this.turnState.currentTurn - attacker.bornOn) < 1 && !(attacker.statuses & Statuses.Charge))
      )
    ){
      this.log.warn('Piece %s must wait a turn to attack', attacker.id);
      queue.push(new Message('Minions need time to prepare!', action.playerId));
      return queue.cancel(action);
    }

    let maxAttacks = (attacker.statuses & Statuses.DyadStrike) ? 2 : 1;
    if(!action.isTauntAttack && attacker.attackCount >= maxAttacks){
      this.log.warn('Piece %s has already attacked', attacker.id);
      queue.push(new Message('Piece has already attacked!', action.playerId));
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
    if(rangedAttack && (attackerTile.position.y - targetTile.position.y) >= Constants.heightDeltaThreshold ){
      bonus = -1;
      bonusMsg = 'High Ground';
    }

    queue.push(new PieceHealthChange({pieceId: action.targetPieceId, change: -attacker.attack, bonus, bonusMsg}));

    //counter attack if in range
    if(!rangedAttack || (target.range != null && target.range >= targetDistance)){
      //if(!action.isTauntAttack || !(target.statuses & Statuses.CantAttack || target.statuses & Statuses.Paralyze)){
      if(!(target.statuses & Statuses.Paralyze)){
        queue.push(new PieceHealthChange({pieceId: action.attackingPieceId, change: -target.attack}));
      }
    }

    //cleave and piercing checks
    let extraHitPositions = null;
    if(attacker.statuses & Statuses.Cleave){
      extraHitPositions = cleavePositions(attacker.position, attacker.direction);
    }
    if(attacker.statuses & Statuses.Piercing){
      extraHitPositions = piercePositions(attacker.position, attacker.direction);
    }
    if(extraHitPositions && extraHitPositions.length){
      var extraHitPieces = extraHitPositions.map(p => this.pieceState.pieceAt(p.x, p.z)).filter(p => p);
      for (const extraHitPiece of extraHitPieces) {
        if(extraHitPiece.playerId === attacker.playerId) continue;

        queue.push(new PieceHealthChange({pieceId: extraHitPiece.id, change: -attacker.attack, bonus, bonusMsg}));
      }

      let clearableTiles = this.mapState.map.tiles.filter(t => t.clearable && extraHitPositions.some(p => p.tileEquals(t.position)));
      if(clearableTiles.length){
        queue.push(new TilesCleared(clearableTiles.map(t => t.position)));
      }
    }

    attacker.attackCount++;
    if(attacker.range != null){
      attacker.moveCount = attacker.movement;
    }

    //Remove cloak once they've attacked
    if(attacker.statuses & Statuses.Cloak){
      queue.push(new PieceStatusChange({pieceId: attacker.id, remove: Statuses.Cloak }));
    }

    this.cardEvaluator.evaluatePieceEvent('attacks', attacker, {targetPieceId: target.id});

    this.log.info('piece %s (%s/%s) attacked %s (%s/%s) direction %s',
      attacker.id, attacker.attack, attacker.health,
      target.id, target.attack, target.health, attackerNewDirection);
    queue.complete(action);
  }
}

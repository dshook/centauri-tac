import GamePiece from '../models/GamePiece.js';
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
  constructor(pieceState, cardEvaluator, mapState)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.mapState = mapState;
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

    let targetDistance = this.mapState.tileDistance(attacker.position, target.position);
    if(targetDistance > 1){
      this.log.warn('Attacker too far away from target %s', targetDistance);
      return queue.cancel(action);
    }

    if(attacker.statuses & Statuses.Paralyze){
      this.log.warn('Cannot attack with piece %s with status %s', attacker.id, attacker.statuses);
      return queue.cancel(action);
    }

    if(target.statuses & Statuses.Cloak){
      this.log.warn('Cannot attack piece %s with Cloak', target.id);
      return queue.cancel(action);
    }

    //determine direction piece should be facing to see if rotation is necessary
    let targetDirection = faceDirection(target.position, attacker.position);
    action.direction = targetDirection;

    let facingDirection = directionOf(targetDirection, target.direction);

    let bonus = 0;
    let bonusMsg = null;
    if(facingDirection == 'behind'){
      this.log.info('Backstab triggered, attackerDirection: %s targetDirection: %s', action.direction, target.direction);
      bonus = -1;
      bonusMsg = 'Backstab';
    }

    queue.push(new PieceHealthChange(action.attackingPieceId, -target.attack));
    queue.push(new PieceHealthChange(action.targetPieceId, -attacker.attack, bonus, bonusMsg));

    //Remove cloak once they've attacked
    if(attacker.statuses & Statuses.Cloak){
      queue.push(new PieceStatusChange(attacker.id, null, Statuses.Cloak ));
    }

    this.cardEvaluator.evaluatePieceEvent('attacks', attacker);

    this.log.info('piece %s (%s/%s) attacked %s (%s/%s)',
      attacker.id, attacker.attack, attacker.health,
      target.id, target.attack, target.health);
    queue.complete(action);
  }
}

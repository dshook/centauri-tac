import GamePiece from '../models/GamePiece.js';
import AttackPiece from '../actions/AttackPiece.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
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
      this.log.info('Attacker or target not found in attack %j', this.pieceState);
      queue.cancel(action);
      return;
    }

    let targetDistance = this.mapState.tileDistance(attacker.position, target.position);
    if(targetDistance > 1){
      this.log.info('Attacker too far away from target %s', targetDistance);
      queue.cancel(action);
    }

    queue.push(new PieceHealthChange(action.attackingPieceId, -target.attack));
    queue.push(new PieceHealthChange(action.targetPieceId, -attacker.attack));

    this.cardEvaluator.evaluatePieceEvent('attacks', attacker);

    this.log.info('piece %s (%s/%s) attacked %s (%s/%s)',
      attacker.id, attacker.attack, attacker.health,
      target.id, target.attack, target.health);
    queue.complete(action);
  }
}

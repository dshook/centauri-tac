import GamePiece from '../models/GamePiece.js';
import AttackPiece from '../actions/AttackPiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class AttackPieceProcessor
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
    if (!(action instanceof AttackPiece)) {
      return;
    }

    //TODO: validate pieces are in range
    var attacker = this.pieceState.pieces.filter(x => x.id == action.attackingPieceId)[0];
    var target = this.pieceState.pieces.filter(x => x.id == action.targetPieceId)[0];

    if(!attacker || !target ){
      this.log.info('Attacker or target not found in attack %j', this.pieceState);
      queue.cancel(action);
      return;
    }

    action.attackerNewHp = attacker.health - target.attack;
    action.targetNewHp = target.health - attacker.attack;

    attacker.health = action.attackerNewHp;
    target.health = action.targetNewHp;

    this.log.info('piece %s (%s/%s) attacked %s (%s/%s)',
      attacker.id, attacker.attack, attacker.health,
      target.id, target.attack, target.health);
    queue.complete(action);
  }
}

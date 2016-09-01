import BaseAction from './BaseAction.js';

/**
 * Attack dat
 */
export default class AttackPiece extends BaseAction
{
  constructor(attackingPieceId, targetPieceId, isTauntAttack = false)
  {
    super();
    this.attackingPieceId = attackingPieceId;
    this.targetPieceId = targetPieceId;
    this.isTauntAttack = isTauntAttack;

    //resulting direction of the attacker
    this.direction = null;
  }
}
